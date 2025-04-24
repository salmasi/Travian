namespace YourProjectName.Services;
// Services/AttackProcessingService.cs
public class AttackProcessingService : BackgroundService
{
    private readonly IServiceProvider _services;

    public AttackProcessingService(IServiceProvider services)
    {
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
            
            var pendingAttacks = await dbContext.Attacks
                .Where(a => a.Outcome == "pending" && a.ArrivalTime <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var attack in pendingAttacks)
            {
                await ProcessAttack(attack, dbContext);
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // هر 30 ثانیه چک کند
        }
    }

    private async Task ProcessAttack(Attack attack, GameDbContext dbContext)
    {
        // 1. دریافت اطلاعات نیروها
        var attackerTroops = await dbContext.AttackTroops
            .Where(t => t.AttackId == attack.Id)
            .ToListAsync();

        var defenderTroops = await dbContext.Troops
            .Where(t => t.VillageId == attack.DefenderVillageId)
            .ToListAsync();

        // 2. شبیه‌سازی نبرد
        var battleResult = SimulateBattle(attackerTroops, defenderTroops);

        // 3. ثبت نتیجه
        attack.Outcome = battleResult.Outcome;
        attack.AttackerLosses = battleResult.AttackerLosses;
        attack.DefenderLosses = battleResult.DefenderLosses;

        // 4. غنیمت‌گیری در صورت پیروزی
        if (attack.Outcome == "victory" && attack.AttackType != "reinforcement")
        {
            var loot = CalculateLoot(attack.DefenderVillageId, dbContext);
            attack.LootWood = loot.Wood;
            attack.LootClay = loot.Clay;
            attack.LootIron = loot.Iron;
            attack.LootCrop = loot.Crop;

            await TransferResources(attack);
        }

        // 5. ایجاد گزارش نبرد
        await CreateBattleReports(attack, battleResult, dbContext);

        await dbContext.SaveChangesAsync();
    }
}