namespace YourProjectName.Services;

// Services/BattleCalculator/IBattleCalculatorService.cs
public interface IBattleCalculatorService
{
    BattleResult CalculateBattleOutcome(List<AttackTroop> attackers, List<VillageTroop> defenders);
    double CalculateResourceLoot(Village village, double lootPercentage);
    BattleSimulation SimulateBattle(BattleScenario scenario);
    int CalculateTravelTime(Coordinate start, Coordinate end, int slowestUnitSpeed);
}

// Services/BattleCalculator/BattleCalculatorService.cs
public class BattleCalculatorService : IBattleCalculatorService
{
    private readonly IConfiguration _config;
    private readonly ILogger<BattleCalculatorService> _logger;

    public BattleCalculatorService(IConfiguration config, ILogger<BattleCalculatorService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public BattleResult CalculateBattleOutcome(List<AttackTroop> attackers, List<VillageTroop> defenders)
    {
        var result = new BattleResult();
        try
        {
            // محاسبات پیچیده نبرد
            var attackerPower = CalculateTotalPower(attackers, false);
            var defenderPower = CalculateTotalPower(defenders, true);
            
            result.Outcome = attackerPower > defenderPower ? BattleOutcome.Victory : BattleOutcome.Defeat;
            result.Casualties = CalculateCasualties(attackers, defenders, attackerPower, defenderPower);
            
            _logger.LogInformation($"Battle calculated - Attack: {attackerPower} vs Defense: {defenderPower}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in battle calculation");
            throw;
        }
        return result;
    }

    private double CalculateTotalPower(IEnumerable<Troop> troops, bool isDefender)
    {
        // محاسبات قدرت بر اساس نوع نیروها و امکانات
        return troops.Sum(t => t.Quantity * GetTroopStats(t.TroopType, isDefender));
    }

    private Dictionary<string, int> CalculateCasualties(...)
    {
        // منطق محاسبه تلفات
    }
}