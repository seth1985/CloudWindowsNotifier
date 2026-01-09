using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WindowsNotifierCloud.Api.Models.Modules;
using WindowsNotifierCloud.Domain.Entities;
using WindowsNotifierCloud.Domain.Interfaces;
using WindowsNotifierCloud.Infrastructure.Persistence;
using WindowsNotifierCloud.Api.Services;

namespace WindowsNotifierCloud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModulesController : ControllerBase
{
    private readonly IModuleRepository _modules;
    private readonly EnvironmentOptions _envOptions;
    private readonly ApplicationDbContext _db;
    private readonly StorageCleanupService _cleanup;
    private readonly MediatR.ISender _sender;

    public ModulesController(IModuleRepository modules, EnvironmentOptions envOptions, ApplicationDbContext db, StorageCleanupService cleanup, MediatR.ISender sender)
    {
        _modules = modules;
        _envOptions = envOptions;
        _db = db;
        _cleanup = cleanup;
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = "BasicOrAdvanced")]
    public async Task<ActionResult<IEnumerable<ModuleDto>>> List(CancellationToken ct)
    {
        var data = await _modules.ListAsync(ct);
        return Ok(data.Select(m => m.ToDto()));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "BasicOrAdvanced")]
    public async Task<ActionResult<ModuleDto>> Get(Guid id, CancellationToken ct)
    {
        var entity = await _modules.GetAsync(id, ct);
        if (entity == null) return NotFound();
        return entity.ToDto();
    }

    [HttpPost]
    [Authorize(Policy = "BasicOrAdvanced")]
    public async Task<ActionResult<ModuleDto>> Create([FromBody] ModuleUpsertRequest request, CancellationToken ct)
    {
        try
        {
            var command = new WindowsNotifierCloud.Api.Features.Modules.CreateModule.Command(request, User);
            var entity = await _sender.Send(command, ct);
            return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity.ToDto());
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "BasicOrAdvanced")]
    public async Task<ActionResult<ModuleDto>> Update(Guid id, [FromBody] ModuleUpsertRequest request, CancellationToken ct)
    {
        try
        {
            var command = new WindowsNotifierCloud.Api.Features.Modules.UpdateModule.Command(id, request, User);
            var entity = await _sender.Send(command, ct);
            
            if (entity == null) return NotFound();
            
            return entity.ToDto();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "BasicOrAdvanced")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            var command = new WindowsNotifierCloud.Api.Features.Modules.DeleteModule.Command(id, User);
            var success = await _sender.Send(command, ct);
            
            if (!success) return NotFound();
            
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }


}
