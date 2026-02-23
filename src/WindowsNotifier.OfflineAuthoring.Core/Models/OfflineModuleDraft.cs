namespace WindowsNotifier.OfflineAuthoring.Core.Models;

public sealed class OfflineModuleDraft
{
    public string ModuleId { get; set; } = string.Empty;
    public OfflineModuleType Type { get; set; } = OfflineModuleType.Standard;
    public string Category { get; set; } = "General";
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? LinkUrl { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ScheduleUtc { get; set; }
    public DateTime? ExpiresUtc { get; set; }

    public string? IconFileName { get; set; }
    public string? IconSourcePath { get; set; }
    public string? HeroFileName { get; set; }
    public string? HeroSourcePath { get; set; }

    public int? ReminderHours { get; set; }

    public string? ConditionalScriptBody { get; set; }
    public int? ConditionalIntervalMinutes { get; set; }

    public string? DynamicScriptBody { get; set; }
    public int DynamicMaxLength { get; set; } = 240;
    public bool DynamicTrimWhitespace { get; set; } = true;
    public bool DynamicFailIfEmpty { get; set; } = true;
    public string? DynamicFallbackMessage { get; set; }

    public CoreSettingsDraft? CoreSettings { get; set; }
}
