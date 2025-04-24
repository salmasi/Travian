using Microsoft.EntityFrameworkCore;
using YourProjectName.Domain.Entities;
using YourProjectName.Domain.Enums;
using YourProjectName.Exceptions;
using YourProjectName.Infrastructure.Data;
using YourProjectName.Models;
using YourProjectName.Repositories;
using YourProjectName.Services.Calculators;

namespace YourProjectName.Services
{
    public class GameService : IGameService
    {
        private readonly AppDbContext _context;
        private readonly IPlayerRepository _playerRepository;
        private readonly IVillageRepository _villageRepository;
        private readonly IAllianceRepository _allianceRepository;
        private readonly IAttackRepository _attackRepository;
        private readonly IResourceCalculator _resourceCalculator;
        private readonly IBattleCalculator _battleCalculator;
        private readonly ILogger<GameService> _logger;

        public GameService(
            AppDbContext context,
            IPlayerRepository playerRepository,
            IVillageRepository villageRepository,
            IAllianceRepository allianceRepository,
            IAttackRepository attackRepository,
            IResourceCalculator resourceCalculator,
            IBattleCalculator battleCalculator,
            ILogger<GameService> logger)
        {
            _context = context;
            _playerRepository = playerRepository;
            _villageRepository = villageRepository;
            _allianceRepository = allianceRepository;
            _attackRepository = attackRepository;
            _resourceCalculator = resourceCalculator;
            _battleCalculator = battleCalculator;
            _logger = logger;
        }

        #region Player Management
        public async Task<PlayerInfoDto> GetPlayerInfoAsync(Guid playerId)
        {
            var player = await _playerRepository.GetByIdAsync(playerId)
                ?? throw new PlayerNotFoundException(playerId);

            return new PlayerInfoDto
            {
                Id = player.Id,
                Username = player.Username,
                Email = player.Email,
                CreatedAt = player.CreatedAt,
                LastLogin = player.LastLogin,
                Villages = await GetPlayerVillagesAsync(playerId)
            };
        }

        public async Task UpdatePlayerResourcesAsync(Guid playerId, ResourceUpdateDto update)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var villages = await _villageRepository.GetByPlayerIdAsync(playerId);
                
                foreach (var village in villages)
                {
                    village.Resources.Wood += update.Wood;
                    village.Resources.Clay += update.Clay;
                    village.Resources.Iron += update.Iron;
                    village.Resources.Crop += update.Crop;
                    
                    await _villageRepository.UpdateAsync(village);
                }
                
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error updating resources for player {playerId}");
                throw;
            }
        }
        #endregion

        #region Village Operations
        public async Task<VillageDto> GetVillageDetailsAsync(Guid villageId)
        {
            var village = await _villageRepository.GetDetailedVillageAsync(villageId)
                ?? throw new VillageNotFoundException(villageId);

            return new VillageDto
            {
                Id = village.Id,
                Name = village.Name,
                Coordinates = new CoordinatesDto(village.X, village.Y),
                Resources = new ResourceDto
                {
                    Wood = village.Resources.Wood,
                    Clay = village.Resources.Clay,
                    Iron = village.Resources.Iron,
                    Crop = village.Resources.Crop,
                    WarehouseCapacity = village.Resources.WarehouseCapacity,
                    GranaryCapacity = village.Resources.GranaryCapacity
                },
                Buildings = village.Buildings.Select(b => new BuildingDto
                {
                    Type = b.Type,
                    Level = b.Level,
                    UpgradeEndTime = b.UpgradeEndTime
                }).ToList(),
                Troops = village.Troops.Select(t => new TroopDto
                {
                    Type = t.Type,
                    Count = t.Count
                }).ToList()
            };
        }

        public async Task<BuildingUpgradeResult> UpgradeBuildingAsync(Guid villageId, BuildingType buildingType)
        {
            var village = await _villageRepository.GetDetailedVillageAsync(villageId)
                ?? throw new VillageNotFoundException(villageId);

            var building = village.Buildings.FirstOrDefault(b => b.Type == buildingType)
                ?? throw new BuildingNotFoundException(buildingType);

            var upgradeCost = _resourceCalculator.CalculateUpgradeCost(buildingType, building.Level + 1);
            
            if (!village.Resources.HasEnough(upgradeCost))
                throw new NotEnoughResourcesException();

            // کسر منابع
            village.Resources.Wood -= upgradeCost.Wood;
            village.Resources.Clay -= upgradeCost.Clay;
            village.Resources.Iron -= upgradeCost.Iron;
            village.Resources.Crop -= upgradeCost.Crop;

            // ارتقاء ساختمان
            building.Level++;
            building.UpgradeStartTime = DateTime.UtcNow;
            building.UpgradeEndTime = DateTime.UtcNow.Add(
                _resourceCalculator.CalculateUpgradeTime(buildingType, building.Level));

            await _villageRepository.UpdateAsync(village);

            return new BuildingUpgradeResult
            {
                Success = true,
                NewLevel = building.Level,
                Duration = building.UpgradeEndTime.Value - DateTime.UtcNow,
                CompletionTime = building.UpgradeEndTime.Value,
                Cost = upgradeCost
            };
        }

        public async Task UpdateVillageResourcesAsync(Guid villageId)
        {
            var village = await _villageRepository.GetDetailedVillageAsync(villageId)
                ?? throw new VillageNotFoundException(villageId);

            var timePassed = DateTime.UtcNow - village.Resources.LastUpdated;
            var productionRates = _resourceCalculator.CalculateProductionRates(village);

            village.Resources.Wood = Math.Min(
                village.Resources.Wood + (int)(productionRates.Wood * timePassed.TotalHours),
                village.Resources.WarehouseCapacity);

            // به روزرسانی سایر منابع به همین صورت...
            village.Resources.LastUpdated = DateTime.UtcNow;

            await _villageRepository.UpdateAsync(village);
        }
        #endregion

        #region Military Operations
        public async Task<AttackResult> LaunchAttackAsync(AttackRequestDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var attackerVillage = await _villageRepository.GetDetailedVillageAsync(request.AttackerVillageId)
                    ?? throw new VillageNotFoundException(request.AttackerVillageId);

                var defenderVillage = await _villageRepository.GetDetailedVillageAsync(request.TargetVillageId)
                    ?? throw new VillageNotFoundException(request.TargetVillageId);

                // بررسی موجودی نیروها
                foreach (var troopRequest in request.Troops)
                {
                    var villageTroop = attackerVillage.Troops.FirstOrDefault(t => t.Type == troopRequest.Key)
                        ?? throw new TroopNotFoundException(troopRequest.Key);

                    if (villageTroop.Count < troopRequest.Value)
                        throw new NotEnoughTroopsException(troopRequest.Key);
                }

                // محاسبه زمان حرکت
                var distance = CalculateDistance(
                    attackerVillage.X, attackerVillage.Y,
                    defenderVillage.X, defenderVillage.Y);

                var slowestSpeed = request.Troops.Keys.Min(t => _battleCalculator.GetTroopSpeed(t));
                var duration = TimeSpan.FromHours(distance / slowestSpeed);

                // ایجاد حمله
                var attack = new Attack
                {
                    AttackerVillageId = request.AttackerVillageId,
                    DefenderVillageId = request.TargetVillageId,
                    Type = request.AttackType,
                    DepartureTime = DateTime.UtcNow,
                    ArrivalTime = DateTime.UtcNow.Add(duration),
                    Status = AttackStatus.Pending
                };

                await _attackRepository.AddAsync(attack);

                // کسر نیروها از روستای حمله کننده
                foreach (var troopRequest in request.Troops)
                {
                    var villageTroop = attackerVillage.Troops.First(t => t.Type == troopRequest.Key);
                    villageTroop.Count -= troopRequest.Value;

                    await _context.AttackTroops.AddAsync(new AttackTroop
                    {
                        AttackId = attack.Id,
                        TroopType = troopRequest.Key,
                        Sent = troopRequest.Value
                    });
                }

                await _villageRepository.UpdateAsync(attackerVillage);
                await transaction.CommitAsync();

                return new AttackResult
                {
                    Success = true,
                    AttackId = attack.Id,
                    ArrivalTime = attack.ArrivalTime
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error launching attack");
                throw;
            }
        }
        #endregion

        #region Helper Methods
        private double CalculateDistance(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }
        #endregion

        // سایر متدها با پیاده‌سازی مشابه...
    }
}