[ApiController]
[Route("api/village/{villageId}/resources")]
public class ResourceController : ControllerBase
{
    private readonly IResourceService _resourceService;

    public ResourceController(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateResources(int villageId)
    {
        await _resourceService.UpdateVillageResources(villageId);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetResources(int villageId)
    {
        var resources = await _resourceService.GetVillageResources(villageId);
        return Ok(resources);
    }
}