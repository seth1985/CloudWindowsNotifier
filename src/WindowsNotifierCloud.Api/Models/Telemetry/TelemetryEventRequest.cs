using System.ComponentModel.DataAnnotations;

namespace WindowsNotifierCloud.Api.Models.Telemetry;

public class TelemetryEventRequest
{
    [Required]
    public string ModuleId { get; set; } = string.Empty;

    [Required]
    public string DeviceId { get; set; } = string.Empty;

    [Required]
    public string UserPrincipalName { get; set; } = string.Empty;

    [Required]
    public string EventType { get; set; } = string.Empty;

    [Required]
    public DateTime OccurredAtUtc { get; set; }

    public Dictionary<string, object>? AdditionalData { get; set; }
}
