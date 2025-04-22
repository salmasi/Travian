// Models/BattleResult.cs
public class BattleResult
{
    public BattleOutcome Outcome { get; set; }
    public double AttackerPower { get; set; }
    public double DefenderPower { get; set; }
    public int AttackerLosses { get; set; }
    public int DefenderLosses { get; set; }
    public Dictionary<string, int> AttackerCasualties { get; set; }
    public Dictionary<string, int> DefenderCasualties { get; set; }
}