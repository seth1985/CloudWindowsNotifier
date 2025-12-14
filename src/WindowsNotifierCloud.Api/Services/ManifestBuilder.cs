using System.Text.Json;
using System.Text.Json.Serialization;
using WindowsNotifierCloud.Domain.Entities;

namespace WindowsNotifierCloud.Api.Services;

public class ManifestBuilder
{
    private readonly JsonSerializerOptions _jsonOptions;

    public ManifestBuilder()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
    }

    public string BuildManifest(ModuleDefinition module)
    {
        var manifest = new ManifestModel
        {
            Id = module.ModuleId,
            Type = module.Type switch
            {
                ModuleType.Standard => "standard",
                ModuleType.Conditional => "conditional",
                ModuleType.Dynamic => "dynamic",
                ModuleType.Hero => "hero",
                ModuleType.CoreSettings => "core_update",
                _ => "standard"
            },
            Category = MapCategory(module.Category),
            Title = module.Title,
            Message = module.Message,
            CreatedUtc = module.CreatedUtc,
            ScheduleUtc = module.ScheduleUtc,
            ExpiresUtc = module.ExpiresUtc,
            Media = new MediaBlock
            {
                Icon = module.Type == ModuleType.Hero ? null : module.IconFileName,
                Hero = module.Type == ModuleType.Hero ? module.HeroFileName : null,
                Link = module.LinkUrl,
                Sound = "windows_default",
                Attachments = Array.Empty<string>()
            },
            Behavior = new BehaviorBlock
            {
                ReminderHours = ParseReminderHours(module.ReminderHours),
                ConditionalScript = module.Type == ModuleType.Conditional ? "conditional.ps1" : null,
                ConditionalIntervalMinutes = module.Type == ModuleType.Conditional ? module.ConditionalIntervalMinutes : null
            },
            Dynamic = module.Type == ModuleType.Dynamic
                ? new DynamicBlock
                {
                    Script = "dynamic.ps1",
                    MaxLength = module.DynamicMaxLength ?? 240,
                    Options = new DynamicOptions
                    {
                        TrimWhitespace = module.DynamicTrimWhitespace ?? true,
                        FailIfEmpty = module.DynamicFailIfEmpty ?? true,
                        FallbackMessage = module.DynamicFallbackMessage
                    }
                }
                : null,
            CoreSettings = module.Type == ModuleType.CoreSettings && module.CoreSettings != null
                ? new CoreSettingsBlockManifest
                {
                    Enabled = module.CoreSettings.Enabled,
                    PollingIntervalSeconds = module.CoreSettings.PollingIntervalSeconds,
                    AutoClearModules = module.CoreSettings.AutoClearModules,
                    SoundEnabled = module.CoreSettings.SoundEnabled,
                    ExitMenuVisible = module.CoreSettings.ExitMenuVisible,
                    StartStopMenuVisible = module.CoreSettings.StartStopMenuVisible,
                    HeartbeatSeconds = module.CoreSettings.HeartbeatSeconds
                }
                : null
        };

        return JsonSerializer.Serialize(manifest, _jsonOptions);
    }

    private sealed class ManifestModel
    {
        public string? Id { get; set; }
        public string? Type { get; set; }
        public string? Category { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime? ScheduleUtc { get; set; }
        public DateTime? ExpiresUtc { get; set; }
        public MediaBlock? Media { get; set; }
        public BehaviorBlock? Behavior { get; set; }
        public DynamicBlock? Dynamic { get; set; }
        public CoreSettingsBlockManifest? CoreSettings { get; set; }
    }

    private sealed class MediaBlock
    {
        public string? Icon { get; set; }
        public string? Hero { get; set; }
        public string? Link { get; set; }
        public string? Sound { get; set; }
        public IEnumerable<string>? Attachments { get; set; }
    }

    private sealed class BehaviorBlock
    {
        public int? ReminderHours { get; set; }
        public string? ConditionalScript { get; set; }
        public int? ConditionalIntervalMinutes { get; set; }
    }

    private sealed class DynamicBlock
    {
        public string? Script { get; set; }
        public int? MaxLength { get; set; }
        public DynamicOptions? Options { get; set; }
    }

    private sealed class DynamicOptions
    {
        public bool? TrimWhitespace { get; set; }
        public bool? FailIfEmpty { get; set; }
        public string? FallbackMessage { get; set; }
    }

    private sealed class CoreSettingsBlockManifest
    {
        public int Enabled { get; set; }
        public int PollingIntervalSeconds { get; set; }
        public int AutoClearModules { get; set; }
        public int SoundEnabled { get; set; }
        public int ExitMenuVisible { get; set; }
        public int StartStopMenuVisible { get; set; }
        public int HeartbeatSeconds { get; set; }
    }

    private static string MapCategory(ModuleCategory category)
    {
        return category switch
        {
            ModuleCategory.GeneralInfo => "General",
            ModuleCategory.Security => "Security",
            ModuleCategory.Compliance => "Compliance",
            ModuleCategory.Maintenance => "Maintenance",
            ModuleCategory.Application => "Application",
            ModuleCategory.Other => "Other",
            _ => "General"
        };
    }

    private static int? ParseReminderHours(string? reminder)
    {
        if (string.IsNullOrWhiteSpace(reminder)) return null;
        if (int.TryParse(reminder, out var val)) return val;
        return null;
    }
}
