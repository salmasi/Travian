public class GameService : IGameService
{
    private readonly GameDbContext _context;

    public GameService(GameDbContext context)
    {
        _context = context;
    }

    public async Task<Village> GetVillage(int villageId)
    {
        return await _context.Villages
            .Include(v => v.Buildings)
            .FirstOrDefaultAsync(v => v.Id == villageId);
    }

    public async Task UpgradeBuilding(int villageId, int buildingId)
    {
        var building = await _context.Buildings
            .FirstOrDefaultAsync(b => b.Id == buildingId && b.VillageId == villageId);

        if (building == null)
            throw new Exception("Building not found");

        building.Level++;
        await _context.SaveChangesAsync();
    }
}