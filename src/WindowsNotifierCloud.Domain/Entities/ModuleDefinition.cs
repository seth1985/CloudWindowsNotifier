namespace WindowsNotifierCloud.Domain.Entities;

public class ModuleDefinition
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string ModuleId { get; set; } = string.Empty;
    public ModuleType Type { get; set; }
    public ModuleCategory Category { get; set; }
    public string? Description { get; set; }

    public Guid? CampaignId { get; set; }
    public Campaign? Campaign { get; set; }

    // Content
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? LinkUrl { get; set; }

    // Scripts
    public string? ConditionalScriptBody { get; set; }
    public int? ConditionalIntervalMinutes { get; set; }
    public string? DynamicScriptBody { get; set; }

    // Scheduling
    public DateTime CreatedUtc { get; set; }
    public DateTime? ScheduleUtc { get; set; }
    public DateTime? ExpiresUtc { get; set; }
    public string? ReminderHours { get; set; }

    // Media
    public string? IconFileName { get; set; }
    public string? IconOriginalName { get; set; }
    public string? HeroFileName { get; set; }
    public string? HeroOriginalName { get; set; }

    // Dynamic options
    public int? DynamicMaxLength { get; set; }
    public bool? DynamicTrimWhitespace { get; set; }
    public bool? DynamicFailIfEmpty { get; set; }
    public string? DynamicFallbackMessage { get; set; }

    // Core settings
    public CoreSettingsBlock? CoreSettings { get; set; }

    // Lifecycle
    public int Version { get; set; }
    public bool IsPublished { get; set; }

    public Guid CreatedByUserId { get; set; }
    public PortalUser? CreatedBy { get; set; }
    public Guid? LastModifiedByUserId { get; set; }
    public PortalUser? LastModifiedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
}
