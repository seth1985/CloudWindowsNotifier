using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WindowsNotifierCloud.Api.Models.Intune;
using WindowsNotifierCloud.Api.Services;

namespace WindowsNotifierCloud.Api.Controllers;

[ApiController]
[Route("api/intune")]
public sealed class IntuneController : ControllerBase
{
    private readonly IntuneDeploymentService _intuneDeployment;

    public IntuneController(IntuneDeploymentService intuneDeployment)
    {
        _intuneDeployment = intuneDeployment;
    }

    [HttpGet("groups")]
    [Authorize(Policy = "AdvancedOnly")]
    public async Task<IActionResult> GetGroups(CancellationToken ct)
    {
        try
        {
            var groups = await _intuneDeployment.GetEligibleGroupsAsync(ct);
            return Ok(groups);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("deploy/{id:guid}")]
    [Authorize(Policy = "AdvancedOnly")]
    public async Task<IActionResult> Deploy(Guid id, [FromBody] IntuneDeployRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _intuneDeployment.DeployToGroupAsync(id, request.GroupId, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
