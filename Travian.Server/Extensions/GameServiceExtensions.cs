public static class GameServiceExtensions
{
    public static ResourceUpdateDto ToDto(this Resources resources)
    {
        return new ResourceUpdateDto
        {
            Wood = resources.Wood,
            Clay = resources.Clay,
            Iron = resources.Iron,
            Crop = resources.Crop
        };
    }
}

public class GameRuleException : Exception
{
    public GameRuleException(string message) : base(message) { }
}