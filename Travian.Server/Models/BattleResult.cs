namespace YourProjectName.Models;


public class BattleResult
{
    public BattleOutcome Outcome { get; set; }
    public Dictionary<string, int> Casualties { get; set; }
    public Dictionary<ResourceType, double> Loot { get; set; }
}

public enum BattleOutcome
{
    Victory,
    Defeat,
    Draw
}