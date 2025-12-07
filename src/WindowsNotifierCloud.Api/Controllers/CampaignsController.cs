using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WindowsNotifierCloud.Api.Models.Campaigns;
using WindowsNotifierCloud.Domain.Entities;
using WindowsNotifierCloud.Domain.Interfaces;
using WindowsNotifierCloud.Infrastructure.Persistence;

namespace WindowsNotifierCloud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CampaignsController : ControllerBase
{
    private readonly ICampaignRepository _campaigns;
    private readonly EnvironmentOptions _envOptions;
    private readonly ApplicationDbContext _db;

    public CampaignsController(ICampaignRepository campaigns, EnvironmentOptions envOptions, ApplicationDbContext db)
    {
        _campaigns = campaigns;
        _envOptions = envOptions;
        _db = db;
    }

    [HttpGet]
    [Authorize(Policy = "BasicOrAdvanced")]
    public async Task<ActionResult<IEnumerable<CampaignDto>>> List(CancellationToken ct)
    {
        var data = await _campaigns.ListAsync(ct);
        return Ok(data.Select(c => c.ToDto()));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "BasicOrAdvanced")]
    public async Task<ActionResult<CampaignDto>> Get(Guid id, CancellationToken ct)
    {
        var entity = await _campaigns.GetAsync(id, ct);
        if (entity == null) return NotFound();
        return entity.ToDto();
    }

    [HttpPost]
    [Authorize(Policy = "AdvancedOnly")]
    public async Task<ActionResult<CampaignDto>> Create([FromBody] CampaignCreateRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        var userId = ResolveUserId();

        var campaign = new Campaign
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        await _campaigns.AddAsync(campaign, ct);
        await _campaigns.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(Get), new { id = campaign.Id }, campaign.ToDto());
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdvancedOnly")]
    public async Task<ActionResult<CampaignDto>> Update(Guid id, [FromBody] CampaignUpdateRequest request, CancellationToken ct)
    {
        var entity = await _campaigns.GetAsync(id, ct);
        if (entity == null) return NotFound();

        entity.Name = request.Name?.Trim() ?? entity.Name;
        entity.Description = request.Description?.Trim();

        await _campaigns.SaveChangesAsync(ct);
        return entity.ToDto();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdvancedOnly")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _campaigns.GetAsync(id, ct);
        if (entity == null) return NotFound();

        // Soft delete not specified; perform hard delete for now.
        // Note: navigation to modules not cascaded here; ensure FK rules in schema.
        _campaigns.Delete(entity);
        await _campaigns.SaveChangesAsync(ct);
        return NoContent();
    }

    private Guid ResolveUserId()
    {
        var sub = User.FindFirst("sub")?.Value;
        if (Guid.TryParse(sub, out var parsed))
        {
            return parsed;
        }

        var fallback = _db.PortalUsers.Select(u => u.Id).FirstOrDefault();
        return fallback != Guid.Empty ? fallback : Guid.NewGuid();
    }
}
