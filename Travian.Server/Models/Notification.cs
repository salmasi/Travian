namespace YourProjectName.Models;

public abstract class Notification
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public string Type { get; protected set; }
    public string Message { get; set; }
}

public class BattleReportNotification : Notification
{
    public BattleReportNotification()
    {
        Type = "battleReport";
    }
    public Guid AttackId { get; set; }
    public string Result { get; set; }
}