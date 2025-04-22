[ApiController]
[Route("api/village/{villageId}")]
public class VillageController : ControllerBase
{
    private readonly IGameService _gameService;

    public VillageController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpGet]
    public async Task<IActionResult> GetVillage(int villageId)
    {
        var village = await _gameService.GetVillage(villageId);
        return Ok(village);
    }

    [HttpPost("buildings/{buildingId}/upgrade")]
    public async Task<IActionResult> UpgradeBuilding(int villageId, int buildingId)
    {
        await _gameService.UpgradeBuilding(villageId, buildingId);
        return Ok();
    }
}