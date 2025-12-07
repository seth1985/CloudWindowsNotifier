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

    public ModulesController(IModuleRepository modules, EnvironmentOptions envOptions, ApplicationDbContext db, StorageCleanupService cleanup)
    {
        _modules = modules;
        _envOptions = envOptions;
        _db = db;
        _cleanup = cleanup;
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
        if (string.IsNullOrWhiteSpace(request.DisplayName) || string.IsNullOrWhiteSpace(request.ModuleId))
            return BadRequest("DisplayName and ModuleId are required.");

        if (request.Type == ModuleType.Hero)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest("Hero notifications require a title.");
            // Hero modules do not support message or icon uploads.
            request.Message = null;
            request.IconFileName = null;
            request.IconOriginalName = null;
        }

        // Role restriction: Basic cannot create Conditional/Dynamic/CoreSettings
        var role = User.FindFirst("role")?.Value;
        var disallowed = (role == "Basic") && request.Type != ModuleType.Standard;
        if (disallowed)
            return Forbid();

        var userId = ResolveUserId();

        var entity = new ModuleDefinition
        {
            Id = Guid.NewGuid(),
            DisplayName = request.DisplayName.Trim(),
            ModuleId = request.ModuleId.Trim(),
            Type = request.Type,
            Category = request.Category,
            Description = request.Description?.Trim(),
            CampaignId = request.CampaignId,
            Title = request.Title,
            Message = request.Message,
            LinkUrl = request.LinkUrl,
            ConditionalScriptBody = request.ConditionalScriptBody,
            ConditionalIntervalMinutes = request.ConditionalIntervalMinutes,
            DynamicScriptBody = request.DynamicScriptBody,
            ScheduleUtc = request.ScheduleUtc,
            ExpiresUtc = request.ExpiresUtc,
            ReminderHours = request.ReminderHours,
            IconFileName = request.IconFileName,
            IconOriginalName = request.IconOriginalName,
            HeroFileName = request.HeroFileName,
            HeroOriginalName = request.HeroOriginalName,
            DynamicMaxLength = request.DynamicMaxLength,
            DynamicTrimWhitespace = request.DynamicTrimWhitespace,
            DynamicFailIfEmpty = request.DynamicFailIfEmpty,
            DynamicFallbackMessage = request.DynamicFallbackMessage,
            CoreSettings = request.CoreSettings,
            CreatedUtc = DateTime.UtcNow,
            Version = 1,
            IsPublished = false,
            CreatedByUserId = userId
        };

        await _modules.AddAsync(entity, ct);
        await _modules.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity.ToDto());
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "BasicOrAdvanced")]
    public async Task<ActionResult<ModuleDto>> Update(Guid id, [FromBody] ModuleUpsertRequest request, CancellationToken ct)
    {
        var entity = await _modules.GetAsync(id, ct);
        if (entity == null) return NotFound();

        var role = User.FindFirst("role")?.Value;
        var disallowed = (role == "Basic") && entity.Type != ModuleType.Standard;
        if (disallowed)
            return Forbid();

        if (request.Type == ModuleType.Hero || entity.Type == ModuleType.Hero)
        {
            if (string.IsNullOrWhiteSpace(request.Title ?? entity.Title))
                return BadRequest("Hero notifications require a title.");
            request.Message = null;
            request.IconFileName = null;
            request.IconOriginalName = null;
        }

        entity.DisplayName = request.DisplayName?.Trim() ?? entity.DisplayName;
        entity.Description = request.Description?.Trim();
        entity.Category = request.Category;
        entity.CampaignId = request.CampaignId;
        entity.Title = request.Title;
        entity.Message = request.Message;
        entity.LinkUrl = request.LinkUrl;
        entity.ConditionalScriptBody = request.ConditionalScriptBody;
        entity.ConditionalIntervalMinutes = request.ConditionalIntervalMinutes;
        entity.DynamicScriptBody = request.DynamicScriptBody;
        entity.ScheduleUtc = request.ScheduleUtc;
        entity.ExpiresUtc = request.ExpiresUtc;
        entity.ReminderHours = request.ReminderHours;
        entity.IconFileName = request.IconFileName;
        entity.IconOriginalName = request.IconOriginalName;
        entity.HeroFileName = request.HeroFileName ?? entity.HeroFileName;
        entity.HeroOriginalName = request.HeroOriginalName ?? entity.HeroOriginalName;
        entity.DynamicMaxLength = request.DynamicMaxLength;
        entity.DynamicTrimWhitespace = request.DynamicTrimWhitespace;
        entity.DynamicFailIfEmpty = request.DynamicFailIfEmpty;
        entity.DynamicFallbackMessage = request.DynamicFallbackMessage;
        entity.CoreSettings = request.CoreSettings;
        entity.LastModifiedAtUtc = DateTime.UtcNow;
        entity.LastModifiedByUserId = ResolveUserId();

        await _modules.SaveChangesAsync(ct);
        return entity.ToDto();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "BasicOrAdvanced")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _modules.GetAsync(id, ct);
        if (entity == null) return NotFound();

        var role = User.FindFirst("role")?.Value;
        var disallowed = (role == "Basic") && entity.Type != ModuleType.Standard;
        if (disallowed)
            return Forbid();

        _modules.Delete(entity);
        await _modules.SaveChangesAsync(ct);

        _cleanup.RemoveModuleArtifacts(entity.Id, entity.ModuleId);
        return NoContent();
    }

    private Guid ResolveUserId()
    {
        var sub = User.FindFirst("sub")?.Value;
        if (Guid.TryParse(sub, out var parsed))
        {
            return parsed;
        }

        // fallback to first user if claims missing
        var fallback = _db.PortalUsers.Select(u => u.Id).FirstOrDefault();
        return fallback != Guid.Empty ? fallback : Guid.NewGuid();
    }
}
