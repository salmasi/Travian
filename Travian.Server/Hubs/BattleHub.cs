using Microsoft.AspNetCore.SignalR;

namespace YourProjectName.Services;
// Hubs/BattleHub.cs
public class BattleHub : Hub
{
    private readonly IGameService _gameService;

    public BattleHub(IGameService gameService)
    {
        _gameService = gameService;
    }

    public async Task SubscribeToVillage(string villageId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, villageId);
    }

    public async Task NotifyAttackResult(string attackId)
    {
        var attack = await _gameService.GetAttack(attackId);
        await Clients.Group(attack.DefenderVillageId.ToString())
            .SendAsync("AttackReceived", attack);
    }
}