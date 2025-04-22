// Controllers/BattleController.cs
[ApiController]
[Route("api/battle")]
[Authorize]
public class BattleController : ControllerBase
{
    private readonly IBattleService _battleService;

    public BattleController(IBattleService battleService)
    {
        _battleService = battleService;
    }

    [HttpPost("attack")]
    public async Task<IActionResult> LaunchAttack([FromBody] AttackRequest request)
    {
        var result = await _battleService.LaunchAttack(request);
        return Ok(result);
    }

    [HttpGet("reports")]
    public async Task<IActionResult> GetBattleReports()
    {
        var playerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var reports = await _battleService.GetPlayerReports(playerId);
        return Ok(reports);
    }
}