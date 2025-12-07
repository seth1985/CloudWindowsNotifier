using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WindowsNotifierCloud.Api.Services;
using WindowsNotifierCloud.Domain.Entities;
using WindowsNotifierCloud.Domain.Interfaces;

namespace WindowsNotifierCloud.Api.Controllers;

[ApiController]
[Route("api/export/{id:guid}/package")]
public class ExportPackageController : ControllerBase
{
    private readonly IModuleRepository _modules;
    private readonly ExportService _exportService;

    public ExportPackageController(IModuleRepository modules, ExportService exportService)
    {
        _modules = modules;
        _exportService = exportService;
    }

    [HttpPost]
    [Authorize(Policy = "BasicOrAdvanced")]
    public async Task<IActionResult> CreatePackage(Guid id, CancellationToken ct)
    {
        var module = await _modules.GetAsync(id, ct);
        if (module == null) return NotFound();

        var role = User.FindFirst("role")?.Value;
        var disallowed = role == "Basic" && module.Type != ModuleType.Standard;
        if (disallowed) return Forbid();

        try
        {
            var result = await _exportService.ExportZipAsync(id, ct);
            if (result == null) return NotFound();

            return Ok(new { path = result.Path, package = result.PackagePath, version = result.Version, warning = result.Warning });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
