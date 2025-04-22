// Services/Resources/IResourceProductionService.cs
public interface IResourceProductionService
{
    Task CalculateVillageProduction(Guid villageId);
    Task UpdateGlobalResources();
    Dictionary<ResourceType, double> GetProductionRates(Village village);
}

// Services/Resources/ResourceProductionService.cs
public class ResourceProductionService : IResourceProductionService, IHostedService
{
    private readonly GameDbContext _context;
    private readonly Timer _timer;

    public ResourceProductionService(GameDbContext context)
    {
        _context = context;
        _timer = new Timer(UpdateResources, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
    }

    public async Task CalculateVillageProduction(Guid villageId)
    {
        var village = await _context.Villages
            .Include(v => v.Buildings)
            .FirstOrDefaultAsync(v => v.Id == villageId);

        var rates = GetProductionRates(village);
        var timePassed = DateTime.UtcNow - village.LastResourceUpdate;

        foreach (var (resourceType, rate) in rates)
        {
            village.Resources[resourceType] += rate * timePassed.TotalHours;
        }

        village.LastResourceUpdate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public Dictionary<ResourceType, double> GetProductionRates(Village village)
    {
        return new Dictionary<ResourceType, double>
        {
            [ResourceType.Wood] = CalculateWoodProduction(village),
            // محاسبات دیگر منابع...
        };
    }

    private void UpdateResources(object state)
    {
        _ = ProcessAllVillages();
    }

    private async Task ProcessAllVillages()
    {
        var villageIds = await _context.Villages
            .Where(v => v.LastResourceUpdate < DateTime.UtcNow.AddMinutes(-1))
            .Select(v => v.Id)
            .ToListAsync();

        foreach (var id in villageIds)
        {
            await CalculateVillageProduction(id);
        }
    }
}