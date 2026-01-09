using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WindowsNotifierCloud.Api.Services;
using WindowsNotifierCloud.Domain.Entities;
using WindowsNotifierCloud.Domain.Interfaces;

namespace WindowsNotifierCloud.Api.Controllers;

[ApiController]
[Route("api/modules/{id:guid}/assets")]
public class ModuleAssetsController : ControllerBase
{
    private readonly IModuleRepository _modules;
    private readonly StorageService _storage;

    public ModuleAssetsController(IModuleRepository modules, StorageService storage)
    {
        _modules = modules;
        _storage = storage;
    }

    [HttpPost("icon")]
    [Authorize(Policy = "BasicOrAdvanced")]
    public async Task<IActionResult> UploadIcon(Guid id, IFormFile file, CancellationToken ct)
    {
        var module = await _modules.GetAsync(id, ct);
        if (module == null) return NotFound();

        var role = User.FindFirst("role")?.Value;
        var disallowed = role == "Basic" && module.Type != ModuleType.Standard;
        if (disallowed) return Forbid();

        if (file == null || file.Length == 0) return BadRequest("File is required.");

        var stored = await _storage.SaveAssetAsync(id, file, ct);
        
        var userId = Guid.Empty;
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(sub, out var parsed)) userId = parsed;

        module.UpdateIcon(stored.FileName, stored.OriginalName, userId);
        await _modules.SaveChangesAsync(ct);

        return Ok(new { fileName = stored.FileName, originalName = stored.OriginalName });
    }
}
