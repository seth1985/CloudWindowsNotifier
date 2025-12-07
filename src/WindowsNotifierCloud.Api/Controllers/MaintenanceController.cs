using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WindowsNotifierCloud.Api.Services;

namespace WindowsNotifierCloud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdvancedOnly")]
public class MaintenanceController : ControllerBase
{
    private readonly StorageCleanupService _cleanup;

    public MaintenanceController(StorageCleanupService cleanup)
    {
        _cleanup = cleanup;
    }

    [HttpPost("storage/cleanup")]
    public async Task<IActionResult> RunStorageCleanup(CancellationToken ct)
    {
        var removed = await _cleanup.RunRetentionAsync(ct);
        return Ok(new { removed });
    }
}
