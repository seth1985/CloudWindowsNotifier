using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WindowsNotifierCloud.Api.Services;
using WindowsNotifierCloud.Domain.Entities;
using WindowsNotifierCloud.Domain.Interfaces;

namespace WindowsNotifierCloud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly IModuleRepository _modules;
    private readonly ExportService _exportService;
    private readonly EnvironmentOptions _env;

    public ExportController(IModuleRepository modules, ExportService exportService, EnvironmentOptions env)
    {
        _modules = modules;
        _exportService = exportService;
        _env = env;
    }

    [HttpPost("{id:guid}")]
    [Authorize(Policy = "BasicOrAdvanced")]
    public async Task<IActionResult> Export(Guid id, CancellationToken ct)
    {
        var module = await _modules.GetAsync(id, ct);
        if (module == null) return NotFound();

        if (IsBasicRestricted(module))
            return Forbid();

        try
        {
            var result = await _exportService.ExportAsync(id, ct);
            if (result == null) return NotFound();

            return Ok(new { path = result.Path, version = result.Version, warning = result.Warning });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/devcore")]
    [Authorize(Policy = "BasicOrAdvanced")]
    public async Task<IActionResult> ExportDevCore(Guid id, CancellationToken ct)
    {
        if (!string.Equals(_env.Mode, "DevelopmentLocal", StringComparison.OrdinalIgnoreCase))
            return Forbid();

        var module = await _modules.GetAsync(id, ct);
        if (module == null) return NotFound();

        if (IsBasicRestricted(module))
            return Forbid();

        try
        {
            var result = await _exportService.ExportToDevCoreAsync(id, ct);
            if (result == null) return NotFound();

            return Ok(new { path = result.Path, devCorePath = result.DevCorePath, version = result.Version, warning = result.Warning });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private bool IsBasicRestricted(ModuleDefinition module)
    {
        var role = User.FindFirst("role")?.Value;
        return role == "Basic" && module.Type != ModuleType.Standard;
    }
}
