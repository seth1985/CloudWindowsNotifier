using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WindowsNotifierCloud.Api.Services;
using WindowsNotifierCloud.Domain.Interfaces;

namespace WindowsNotifierCloud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ManifestController : ControllerBase
{
    private readonly IModuleRepository _modules;
    private readonly ManifestBuilder _builder;

    public ManifestController(IModuleRepository modules, ManifestBuilder builder)
    {
        _modules = modules;
        _builder = builder;
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "BasicOrAdvanced")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var module = await _modules.GetAsync(id, ct);
        if (module == null) return NotFound();

        var json = _builder.BuildManifest(module);
        return Content(json, "application/json");
    }

    [HttpGet("{moduleId}/preview")]
    [Authorize(Policy = "BasicOrAdvanced")]
    public async Task<IActionResult> PreviewByModuleId(string moduleId, CancellationToken ct)
    {
        var module = await _modules.GetByModuleIdAsync(moduleId, ct);
        if (module == null) return NotFound();

        var json = _builder.BuildManifest(module);
        return Content(json, "application/json");
    }
}
