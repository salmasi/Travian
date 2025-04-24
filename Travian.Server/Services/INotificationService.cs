namespace YourProjectName.Services;

// Services/Notification/INotificationService.cs
public interface INotificationService
{
    Task SendToUser(Guid userId, Notification notification);
    Task BroadcastToVillage(Guid villageId, Notification notification);
    Task SendBattleReport(Guid attackId);
}

// Services/Notification/NotificationService.cs
public class NotificationService : INotificationService
{
    private readonly IHubContext<GameHub> _hubContext;
    private readonly GameDbContext _context;

    public NotificationService(IHubContext<GameHub> hubContext, GameDbContext context)
    {
        _hubContext = hubContext;
        _context = context;
    }

    public async Task SendToUser(Guid userId, Notification notification)
    {
        var connectionIds = await _context.UserConnections
            .Where(c => c.UserId == userId)
            .Select(c => c.ConnectionId)
            .ToListAsync();

        foreach (var connectionId in connectionIds)
        {
            await _hubContext.Clients.Client(connectionId)
                .SendAsync("ReceiveNotification", notification);
        }
    }

    public async Task SendBattleReport(Guid attackId)
    {
        var attack = await _context.Attacks
            .Include(a => a.AttackerVillage)
            .Include(a => a.DefenderVillage)
            .FirstOrDefaultAsync(a => a.Id == attackId);

        var report = new BattleReportNotification
        {
            AttackId = attackId,
            Result = attack.Outcome.ToString(),
            // سایر جزئیات
        };

        await SendToUser(attack.AttackerVillage.PlayerId, report);
        await SendToUser(attack.DefenderVillage.PlayerId, report);
    }
}