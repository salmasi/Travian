// Services/ResourceService.cs
public class ResourceService : IResourceService
{
    private readonly GameDbContext _context;
    
    public ResourceService(GameDbContext context)
    {
        _context = context;
    }

    public async Task UpdateVillageResources(int villageId)
    {
        var village = await _context.Villages
            .Include(v => v.Buildings)
            .FirstOrDefaultAsync(v => v.Id == villageId);

        var now = DateTime.UtcNow;
        var timePassed = now - village.LastResourceUpdate;

        foreach (var building in village.Buildings)
        {
            var productionRate = await _context.BuildingProductionRates
                .FirstOrDefaultAsync(r => r.BuildingType == building.BuildingType);

            if (productionRate != null)
            {
                var rate = productionRate.BaseRate + 
                          (productionRate.RateIncreasePerLevel * building.Level);
                
                var resourceType = GetResourceType(building.BuildingType);
                village.Resources[resourceType] += rate * (decimal)timePassed.TotalMinutes;
            }
        }

        village.LastResourceUpdate = now;
        await _context.SaveChangesAsync();
    }

    private string GetResourceType(string buildingType)
    {
        return buildingType switch
        {
            "woodcutter" => "wood",
            "clay_pit" => "clay",
            "iron_mine" => "iron",
            "crop_farm" => "crop",
            _ => throw new ArgumentException("Invalid building type")
        };
    }
}