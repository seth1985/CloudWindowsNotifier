using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WindowsNotifierCloud.Api.Models.Telemetry;
using WindowsNotifierCloud.Domain.Entities;
using WindowsNotifierCloud.Domain.Interfaces;

namespace WindowsNotifierCloud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TelemetryController : ControllerBase
{
    private readonly ITelemetryRepository _telemetry;
    private readonly TelemetryOptions _options;

    public TelemetryController(ITelemetryRepository telemetry, TelemetryOptions options)
    {
        _telemetry = telemetry;
        _options = options;
    }

    [HttpPost("events")]
    public async Task<IActionResult> Ingest([FromBody] TelemetryEventRequest request, CancellationToken ct)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!Enum.TryParse<TelemetryEventType>(request.EventType, ignoreCase: true, out var evtType))
        {
            return BadRequest("Invalid eventType.");
        }

        var additional = request.AdditionalData is null ? null : JsonSerializer.Serialize(request.AdditionalData);

        var evt = new TelemetryEvent
        {
            Id = Guid.NewGuid(),
            ModuleId = request.ModuleId,
            DeviceId = request.DeviceId,
            UserPrincipalName = request.UserPrincipalName,
            EventType = evtType,
            OccurredAtUtc = request.OccurredAtUtc,
            AdditionalDataJson = additional
        };

        await _telemetry.AddAsync(evt, ct);
        await _telemetry.SaveChangesAsync(ct);

        return Accepted(new { id = evt.Id });
    }

    private bool IsAuthorized()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return false;
        }

        if (!Request.Headers.TryGetValue("x-wn-api-key", out var header))
        {
            return false;
        }

        return string.Equals(header.ToString(), _options.ApiKey, StringComparison.Ordinal);
    }
}
