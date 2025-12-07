namespace WindowsNotifierCloud.Domain.Entities;

public enum TelemetryEventType
{
    ToastShown,
    ButtonOk,
    ButtonMoreInfo,
    Dismissed,
    TimedOut,
    ScriptError,
    ConditionCheck,
    Completed
}

public class TelemetryEvent
{
    public Guid Id { get; set; }
    public string ModuleId { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string UserPrincipalName { get; set; } = string.Empty;
    public TelemetryEventType EventType { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public string? AdditionalDataJson { get; set; }
}
