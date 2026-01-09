namespace WindowsNotifierCloud.Domain.Entities;

public class ModuleDefinition
{
    public Guid Id { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public string ModuleId { get; private set; } = string.Empty;
    public ModuleType Type { get; private set; }
    public ModuleCategory Category { get; private set; }
    public string? Description { get; private set; }

    public Guid? CampaignId { get; private set; }
    public Campaign? Campaign { get; private set; }

    // Content
    public string? Title { get; private set; }
    public string? Message { get; private set; }
    public string? LinkUrl { get; private set; }

    // Scripts
    public string? ConditionalScriptBody { get; private set; }
    public int? ConditionalIntervalMinutes { get; private set; }
    public string? DynamicScriptBody { get; private set; }

    // Scheduling
    public DateTime CreatedUtc { get; private set; }
    public DateTime? ScheduleUtc { get; private set; }
    public DateTime? ExpiresUtc { get; private set; }
    public string? ReminderHours { get; private set; }

    // Media
    public string? IconFileName { get; private set; }
    public string? IconOriginalName { get; private set; }
    public string? HeroFileName { get; private set; }
    public string? HeroOriginalName { get; private set; }

    // Dynamic options
    public int? DynamicMaxLength { get; private set; }
    public bool? DynamicTrimWhitespace { get; private set; }
    public bool? DynamicFailIfEmpty { get; private set; }
    public string? DynamicFallbackMessage { get; private set; }

    // Core settings
    public CoreSettingsBlock? CoreSettings { get; private set; }

    // Lifecycle
    public int Version { get; private set; }
    public bool IsPublished { get; private set; }

    public Guid CreatedByUserId { get; private set; }
    public PortalUser? CreatedBy { get; private set; }
    public Guid? LastModifiedByUserId { get; private set; }
    public PortalUser? LastModifiedBy { get; private set; }
    public DateTime? LastModifiedAtUtc { get; private set; }

    // EF Core requires a parameterless constructor
    private ModuleDefinition() { }

    public static ModuleDefinition Create(
        string displayName,
        string moduleId,
        ModuleType type,
        ModuleCategory category,
        Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Display Name is required.");
        if (string.IsNullOrWhiteSpace(moduleId)) throw new ArgumentException("Module ID is required.");

        return new ModuleDefinition
        {
            Id = Guid.NewGuid(),
            DisplayName = displayName.Trim(),
            ModuleId = moduleId.Trim(),
            Type = type,
            Category = category,
            CreatedByUserId = createdByUserId,
            CreatedUtc = DateTime.UtcNow,
            Version = 1,
            IsPublished = false
        };
    }

    public void UpdateDetails(string displayName, string? description, ModuleCategory category, Guid modifiedByUserId)
    {
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Display Name is required.");
        DisplayName = displayName.Trim();
        Description = description?.Trim();
        Category = category;
        SetModified(modifiedByUserId);
    }

    public void UpdateContent(string? title, string? message, string? linkUrl, Guid modifiedByUserId)
    {
        if (Type == ModuleType.Hero && string.IsNullOrWhiteSpace(title))
             throw new ArgumentException("Hero notifications require a title.");

        Title = title;
        Message = message;
        LinkUrl = linkUrl;
        SetModified(modifiedByUserId);
    }

    public void UpdateScripts(string? conditionalScript, int? conditionalInterval, string? dynamicScript, Guid modifiedByUserId)
    {
        ConditionalScriptBody = conditionalScript;
        ConditionalIntervalMinutes = conditionalInterval;
        DynamicScriptBody = dynamicScript;
        SetModified(modifiedByUserId);
    }

    public void UpdateSchedule(DateTime? scheduleUtc, DateTime? expiresUtc, string? reminderHours, Guid modifiedByUserId)
    {
        ScheduleUtc = scheduleUtc;
        ExpiresUtc = expiresUtc;
        ReminderHours = reminderHours;
        SetModified(modifiedByUserId);
    }

    public void UpdateMedia(string? iconFile, string? iconOriginal, string? heroFile, string? heroOriginal, Guid modifiedByUserId)
    {
        IconFileName = iconFile;
        IconOriginalName = iconOriginal;
        HeroFileName = heroFile;
        HeroOriginalName = heroOriginal;
        SetModified(modifiedByUserId);
    }

    public void UpdateIcon(string? iconFile, string? iconOriginal, Guid modifiedByUserId)
    {
        IconFileName = iconFile;
        IconOriginalName = iconOriginal;
        SetModified(modifiedByUserId);
    }

    public void UpdateHero(string? heroFile, string? heroOriginal, Guid modifiedByUserId)
    {
        HeroFileName = heroFile;
        HeroOriginalName = heroOriginal;
        SetModified(modifiedByUserId);
    }

    public void UpdateDynamicOptions(int? maxLength, bool? trim, bool? failEmpty, string? fallback, Guid modifiedByUserId)
    {
        DynamicMaxLength = maxLength;
        DynamicTrimWhitespace = trim;
        DynamicFailIfEmpty = failEmpty;
        DynamicFallbackMessage = fallback;
        SetModified(modifiedByUserId);
    }

    public void UpdateCoreSettings(CoreSettingsBlock? settings, Guid modifiedByUserId)
    {
        CoreSettings = settings;
        SetModified(modifiedByUserId);
    }

    private void SetModified(Guid userId)
    {
        LastModifiedByUserId = userId;
        LastModifiedAtUtc = DateTime.UtcNow;
    }
}
