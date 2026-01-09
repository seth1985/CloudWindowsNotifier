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
    private readonly MediatR.ISender _sender;

    public CampaignsController(ICampaignRepository campaigns, MediatR.ISender sender)
    {
        _campaigns = campaigns;
        _sender = sender;
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
        try
        {
            var command = new WindowsNotifierCloud.Api.Features.Campaigns.CreateCampaign.Command(request, User);
            var campaign = await _sender.Send(command, ct);
            return CreatedAtAction(nameof(Get), new { id = campaign.Id }, campaign.ToDto());
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdvancedOnly")]
    public async Task<ActionResult<CampaignDto>> Update(Guid id, [FromBody] CampaignUpdateRequest request, CancellationToken ct)
    {
        try
        {
            var command = new WindowsNotifierCloud.Api.Features.Campaigns.UpdateCampaign.Command(id, request);
            var entity = await _sender.Send(command, ct);
            
            if (entity == null) return NotFound();
            return entity.ToDto();
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdvancedOnly")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var command = new WindowsNotifierCloud.Api.Features.Campaigns.DeleteCampaign.Command(id);
        var success = await _sender.Send(command, ct);
        
        if (!success) return NotFound();
        return NoContent();
    }
}
