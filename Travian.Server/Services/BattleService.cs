using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YourProjectName.Domain.Entities;
using YourProjectName.Models;

namespace YourProjectName.Services
{
    public interface IBattleService
    {
        Task<AttackResult> LaunchAttack(AttackRequest request);
        Task ProcessPendingAttacks();
        Task<BattleReport> GetBattleReport(Guid attackId);
    }

    public class BattleService : IBattleService
    {
        private readonly GameDbContext _context;
        private readonly IResourceService _resourceService;
        private readonly INotificationService _notificationService;
        private readonly IBattleCalculator _battleCalculator;

        public BattleService(
            GameDbContext context,
            IResourceService resourceService,
            INotificationService notificationService,
            IBattleCalculator battleCalculator)
        {
            _context = context;
            _resourceService = resourceService;
            _notificationService = notificationService;
            _battleCalculator = battleCalculator;
        }

        public async Task<AttackResult> LaunchAttack(AttackRequest request)
        {
            // 1. اعتبارسنجی اولیه
            if (request.Troops.Sum(t => t.Quantity) <= 0)
                throw new ArgumentException("حداقل یک نیرو باید انتخاب شود");

            // 2. دریافت اطلاعات روستاها
            var attackerVillage = await GetVillageWithTroops(request.AttackerVillageId);
            var defenderVillage = await GetVillageWithTroops(request.DefenderVillageId);

            // 3. بررسی موجودی نیروها
            ValidateTroopAvailability(attackerVillage, request.Troops);

            // 4. محاسبه زمان حرکت
            var (departureTime, arrivalTime) = CalculateMovementTime(
                attackerVillage, 
                defenderVillage, 
                request.Troops
            );

            // 5. ثبت حمله در دیتابیس
            var attack = await CreateAttackRecord(
                request, 
                departureTime, 
                arrivalTime
            );

            // 6. کسر نیروها از روستای حمله کننده
            await DeductAttackerTroops(attackerVillage, request.Troops);

            // 7. اطلاع‌رسانی بلادرنگ
            await _notificationService.NotifyNewAttack(
                defenderVillage.PlayerId, 
                attack.Id
            );

            return new AttackResult
            {
                Success = true,
                AttackId = attack.Id,
                ArrivalTime = arrivalTime,
                Distance = CalculateDistance(attackerVillage, defenderVillage)
            };
        }

        public async Task ProcessPendingAttacks()
        {
            var pendingAttacks = await _context.Attacks
                .Include(a => a.AttackerTroops)
                .Where(a => a.Outcome == BattleOutcome.Pending.ToString() && 
                           a.ArrivalTime <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var attack in pendingAttacks)
            {
                await ProcessSingleAttack(attack);
            }
        }

        public async Task<BattleReport> GetBattleReport(Guid attackId)
        {
            return await _context.BattleReports
                .FirstOrDefaultAsync(r => r.AttackId == attackId);
        }

        #region Private Methods

        private async Task<Village> GetVillageWithTroops(Guid villageId)
        {
            return await _context.Villages
                .Include(v => v.Troops)
                .Include(v => v.Player)
                .FirstOrDefaultAsync(v => v.Id == villageId) 
                ?? throw new ArgumentException("روستا یافت نشد");
        }

        private void ValidateTroopAvailability(Village village, List<AttackTroopRequest> requestedTroops)
        {
            foreach (var requestedTroop in requestedTroops)
            {
                var villageTroop = village.Troops
                    .FirstOrDefault(t => t.TroopType == requestedTroop.TroopType);

                if (villageTroop == null || villageTroop.Quantity < requestedTroop.Quantity)
                    throw new ArgumentException($"نیروی {requestedTroop.TroopType} به اندازه کافی موجود نیست");
            }
        }

        private (DateTime departureTime, DateTime arrivalTime) CalculateMovementTime(
            Village attacker, 
            Village defender, 
            List<AttackTroopRequest> troops)
        {
            var distance = CalculateDistance(attacker, defender);
            var slowestSpeed = troops
                .Select(t => _battleCalculator.GetTroopSpeed(t.TroopType))
                .Min();

            var hours = distance / slowestSpeed;
            var departure = DateTime.UtcNow;
            var arrival = departure.AddHours(hours);

            return (departure, arrival);
        }

        private double CalculateDistance(Village attacker, Village defender)
        {
            return Math.Sqrt(
                Math.Pow(attacker.X - defender.X, 2) + 
                Math.Pow(attacker.Y - defender.Y, 2)
            );
        }

        private async Task<Attack> CreateAttackRecord(
            AttackRequest request, 
            DateTime departureTime, 
            DateTime arrivalTime)
        {
            var attack = new Attack
            {
                AttackerVillageId = request.AttackerVillageId,
                DefenderVillageId = request.DefenderVillageId,
                AttackType = request.AttackType,
                DepartureTime = departureTime,
                ArrivalTime = arrivalTime,
                Outcome = BattleOutcome.Pending.ToString()
            };

            await _context.Attacks.AddAsync(attack);
            await _context.SaveChangesAsync();

            // ثبت نیروهای حمله کننده
            foreach (var troop in request.Troops)
            {
                await _context.AttackTroops.AddAsync(new AttackTroop
                {
                    AttackId = attack.Id,
                    TroopType = troop.TroopType,
                    Quantity = troop.Quantity
                });
            }

            await _context.SaveChangesAsync();
            return attack;
        }

        private async Task DeductAttackerTroops(Village village, List<AttackTroopRequest> troops)
        {
            foreach (var troop in troops)
            {
                var villageTroop = village.Troops
                    .First(t => t.TroopType == troop.TroopType);
                
                villageTroop.Quantity -= troop.Quantity;
            }

            await _context.SaveChangesAsync();
        }

        private async Task ProcessSingleAttack(Attack attack)
        {
            try
            {
                // 1. دریافت اطلاعات کامل حمله
                var fullAttack = await _context.Attacks
                    .Include(a => a.AttackerTroops)
                    .Include(a => a.DefenderVillage)
                        .ThenInclude(v => v.Troops)
                    .FirstOrDefaultAsync(a => a.Id == attack.Id);

                // 2. محاسبه نتیجه نبرد
                var battleResult = _battleCalculator.CalculateBattleOutcome(
                    fullAttack.AttackerTroops.ToList(),
                    fullAttack.DefenderVillage.Troops.ToList()
                );

                // 3. اعمال نتیجه به دیتابیس
                await ApplyBattleResult(fullAttack, battleResult);

                // 4. ایجاد گزارش نبرد
                await CreateBattleReport(fullAttack, battleResult);

                // 5. ارسال نوتیفیکیشن
                await _notificationService.NotifyBattleResult(
                    fullAttack.AttackerVillageId,
                    fullAttack.DefenderVillageId,
                    battleResult
                );
            }
            catch (Exception ex)
            {
                // لاگ کردن خطا
                Console.WriteLine($"Error processing attack {attack.Id}: {ex.Message}");
            }
        }

        private async Task ApplyBattleResult(Attack attack, BattleResult result)
        {
            // به‌روزرسانی وضعیت حمله
            attack.Outcome = result.Outcome.ToString();
            attack.AttackerLosses = result.AttackerLosses;
            attack.DefenderLosses = result.DefenderLosses;
            attack.ProcessedAt = DateTime.UtcNow;

            // کسر تلفات از نیروهای مدافع
            foreach (var troopLoss in result.DefenderCasualties)
            {
                var troop = attack.DefenderVillage.Troops
                    .FirstOrDefault(t => t.TroopType == troopLoss.Key);
                
                if (troop != null)
                {
                    troop.Quantity = Math.Max(0, troop.Quantity - troopLoss.Value);
                }
            }

            // غنیمت‌گیری در صورت پیروزی
            if (result.Outcome == BattleOutcome.Victory && 
                attack.AttackType != AttackType.Reinforcement.ToString())
            {
                await LootResources(attack);
            }

            await _context.SaveChangesAsync();
        }

        private async Task LootResources(Attack attack)
        {
            var defenderResources = await _context.Resources
                .FirstOrDefaultAsync(r => r.VillageId == attack.DefenderVillageId);

            var lootPercentage = 0.3m; // 30% غنیمت
            
            attack.LootWood = defenderResources.Wood * lootPercentage;
            attack.LootClay = defenderResources.Clay * lootPercentage;
            attack.LootIron = defenderResources.Iron * lootPercentage;
            attack.LootCrop = defenderResources.Crop * lootPercentage;

            // کسر از مدافع
            defenderResources.Wood -= attack.LootWood;
            defenderResources.Clay -= attack.LootClay;
            defenderResources.Iron -= attack.LootIron;
            defenderResources.Crop -= attack.LootCrop;

            // اضافه کردن به حمله کننده
            var attackerResources = await _context.Resources
                .FirstOrDefaultAsync(r => r.VillageId == attack.AttackerVillageId);
            
            attackerResources.Wood += attack.LootWood;
            attackerResources.Clay += attack.LootClay;
            attackerResources.Iron += attack.LootIron;
            attackerResources.Crop += attack.LootCrop;
        }

        private async Task CreateBattleReport(Attack attack, BattleResult result)
        {
            var report = new BattleReport
            {
                AttackId = attack.Id,
                AttackerVillageId = attack.AttackerVillageId,
                DefenderVillageId = attack.DefenderVillageId,
                Outcome = result.Outcome.ToString(),
                ReportData = GenerateReportJson(attack, result),
                CreatedAt = DateTime.UtcNow
            };

            await _context.BattleReports.AddAsync(report);
            await _context.SaveChangesAsync();
        }

        private string GenerateReportJson(Attack attack, BattleResult result)
        {
            // ساخت JSON جزئیات گزارش
            return System.Text.Json.JsonSerializer.Serialize(new
            {
                AttackerTroops = attack.AttackerTroops.Select(t => new {
                    Type = t.TroopType,
                    Sent = t.Quantity,
                    Lost = result.AttackerCasualties.GetValueOrDefault(t.TroopType, 0)
                }),
                DefenderTroops = attack.DefenderVillage.Troops.Select(t => new {
                    Type = t.TroopType,
                    Defended = t.Quantity,
                    Lost = result.DefenderCasualties.GetValueOrDefault(t.TroopType, 0)
                }),
                ResourcesLooted = new {
                    Wood = attack.LootWood,
                    Clay = attack.LootClay,
                    Iron = attack.LootIron,
                    Crop = attack.LootCrop
                },
                BattleStats = new {
                    AttackerPower = result.AttackerPower,
                    DefenderPower = result.DefenderPower
                }
            });
        }

        #endregion
    }

    public enum BattleOutcome { Pending, Victory, Defeat, Draw }
    public enum AttackType { Raid, Attack, Reinforcement }
}