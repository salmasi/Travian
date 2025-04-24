using YourProjectName.Models;
using YourProjectName.Models.Game;
using YourProjectName.Models.Players;
using YourProjectName.Models.Villages;

namespace YourProjectName.Services
{
    public interface IGameService
    {
        #region Player Management
        Task<PlayerInfoDto> GetPlayerInfoAsync(Guid playerId);
        Task UpdatePlayerResourcesAsync(Guid playerId, ResourceUpdateDto update);
        Task<List<PlayerSearchResultDto>> SearchPlayersAsync(string query);
        #endregion

        #region Village Operations
        Task<VillageDto> GetVillageDetailsAsync(Guid villageId);
        Task<List<VillageSimpleDto>> GetPlayerVillagesAsync(Guid playerId);
        Task<BuildingUpgradeResult> UpgradeBuildingAsync(Guid villageId, BuildingType buildingType);
        Task<ResourceProductionDto> GetVillageProductionAsync(Guid villageId);
        Task UpdateVillageResourcesAsync(Guid villageId);
        #endregion

        #region Military Operations
        Task<ArmyInfoDto> GetVillageArmyInfoAsync(Guid villageId);
        Task<TrainTroopsResult> TrainTroopsAsync(Guid villageId, Dictionary<TroopType, int> troops);
        Task<AttackResult> LaunchAttackAsync(AttackRequestDto request);
        Task<List<AttackReportDto>> GetAttackReportsAsync(Guid playerId, int count = 10);
        Task CancelAttackAsync(Guid attackId, Guid playerId);
        #endregion

        #region Alliance Operations
        Task<AllianceInfoDto> GetAllianceInfoAsync(Guid allianceId);
        Task<AllianceCreationResult> CreateAllianceAsync(Guid playerId, AllianceCreationDto dto);
        Task<AllianceApplicationResult> ApplyToAllianceAsync(Guid playerId, Guid allianceId);
        Task ProcessAllianceApplicationAsync(Guid allianceId, Guid applicantId, bool accept);
        Task<List<AllianceMemberDto>> GetAllianceMembersAsync(Guid allianceId);
        #endregion

        #region Map & Navigation
        Task<MapTileDto> GetMapTileAsync(int x, int y);
        Task<List<MapTileDto>> GetMapRegionAsync(int x, int y, int radius);
        Task<List<VillageSearchResultDto>> SearchVillagesAsync(string query);
        #endregion

        #region Game Mechanics
        Task ProcessGameTickAsync();
        Task ResetProductionCycleAsync();
        Task ProcessPendingAttacksAsync();
        Task CleanupInactivePlayersAsync();
        #endregion

        #region Market & Trading
        Task<MarketOfferDto> CreateMarketOfferAsync(MarketOfferCreateDto dto);
        Task<List<MarketOfferDto>> GetMarketOffersAsync(MarketOfferFilter filter);
        Task CancelMarketOfferAsync(Guid offerId, Guid playerId);
        Task ExecuteTradeAsync(Guid offerId, Guid buyerId);
        #endregion

        #region Reports & Notifications
        Task<List<GameNotificationDto>> GetPlayerNotificationsAsync(Guid playerId);
        Task MarkNotificationAsReadAsync(Guid notificationId);
        Task<List<BattleReportDto>> GetBattleReportsAsync(Guid playerId, int count = 10);
        #endregion
    }

    #region DTO Definitions
    public class ResourceUpdateDto
    {
        public int Wood { get; set; }
        public int Clay { get; set; }
        public int Iron { get; set; }
        public int Crop { get; set; }
    }

    public class BuildingUpgradeResult
    {
        public bool Success { get; set; }
        public int NewLevel { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime CompletionTime { get; set; }
        public ResourceUpdateDto Cost { get; set; }
    }

    public class AttackRequestDto
    {
        public Guid AttackerVillageId { get; set; }
        public Guid TargetVillageId { get; set; }
        public Dictionary<TroopType, int> Troops { get; set; }
        public AttackType AttackType { get; set; }
    }

    // سایر DTO ها با توجه به نیازهای پروژه
    #endregion
}