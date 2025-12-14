using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WindowsNotifierCloud.Api.Models.Templates;
using WindowsNotifierCloud.Domain.Entities;
using WindowsNotifierCloud.Infrastructure.Persistence;

namespace WindowsNotifierCloud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "BasicOrAdvanced")]
public class TemplatesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<TemplatesController> _logger;

    public TemplatesController(ApplicationDbContext db, ILogger<TemplatesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TemplateDto>>> List([FromQuery] string? type, CancellationToken ct)
    {
        TemplateType? filter = type?.ToLowerInvariant() switch
        {
            "conditional" => TemplateType.Conditional,
            "dynamic" => TemplateType.Dynamic,
            _ => null
        };

        var query = _db.PowerShellTemplates.AsNoTracking();

        if (filter.HasValue)
        {
            query = query.Where(t => t.Type == TemplateType.Both || t.Type == filter.Value);
        }

        var list = await query
            .OrderBy(t => t.Category)
            .ThenBy(t => t.Title)
            .Include(t => t.CreatedBy)
            .ToListAsync(ct);

        return Ok(list.Select(t => t.ToDto()));
    }

    [HttpPost]
    public async Task<ActionResult<TemplateDto>> Create([FromBody] TemplateCreateRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (string.IsNullOrWhiteSpace(request.ScriptBody))
            return BadRequest("ScriptBody is required.");

        var userId = ResolveUserId();

        var entity = new PowerShellTemplate
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Category = request.Category.Trim(),
            ScriptBody = request.ScriptBody,
            Type = request.Type,
            CreatedUtc = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        _db.PowerShellTemplates.Add(entity);
        await _db.SaveChangesAsync(ct);

        var dto = await _db.PowerShellTemplates
            .Include(t => t.CreatedBy)
            .Where(t => t.Id == entity.Id)
            .Select(t => t.ToDto())
            .FirstAsync(ct);

        return CreatedAtAction(nameof(List), new { id = entity.Id }, dto);
    }

    private Guid ResolveUserId()
    {
        var sub = User?.FindFirst("sub")?.Value;
        if (Guid.TryParse(sub, out var g)) return g;

        var fallback = _db.PortalUsers.Select(u => u.Id).FirstOrDefault();
        return fallback != Guid.Empty ? fallback : Guid.NewGuid();
    }
}
