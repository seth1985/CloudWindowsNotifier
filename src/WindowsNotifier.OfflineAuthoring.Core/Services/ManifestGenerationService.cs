using System.Text.Json;
using System.Text.Json.Serialization;
using WindowsNotifier.OfflineAuthoring.Core.Models;

namespace WindowsNotifier.OfflineAuthoring.Core.Services;

public sealed class ManifestGenerationService
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public string BuildManifestJson(OfflineModuleDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        if (string.IsNullOrWhiteSpace(draft.ModuleId))
        {
            throw new InvalidOperationException("ModuleId is required.");
        }

        var manifest = new ManifestModel
        {
            Id = draft.ModuleId,
            Type = MapType(draft.Type),
            Category = string.IsNullOrWhiteSpace(draft.Category) ? "General" : draft.Category,
            Title = draft.Title,
            Message = draft.Message,
            CreatedUtc = draft.CreatedUtc,
            ScheduleUtc = draft.ScheduleUtc,
            ExpiresUtc = draft.ExpiresUtc,
            Media = new MediaBlock
            {
                Icon = draft.Type == OfflineModuleType.Hero ? null : draft.IconFileName,
                Hero = draft.Type == OfflineModuleType.Hero ? draft.HeroFileName : null,
                Link = draft.LinkUrl,
                Sound = "windows_default",
                Attachments = Array.Empty<string>()
            },
            Behavior = new BehaviorBlock
            {
                ReminderHours = draft.ReminderHours,
                ConditionalScript = draft.Type == OfflineModuleType.Conditional ? "conditional.ps1" : null,
                ConditionalIntervalMinutes = draft.Type == OfflineModuleType.Conditional ? draft.ConditionalIntervalMinutes : null
            },
            Dynamic = draft.Type == OfflineModuleType.Dynamic
                ? new DynamicBlock
                {
                    Script = "dynamic.ps1",
                    MaxLength = draft.DynamicMaxLength,
                    Options = new DynamicOptions
                    {
                        TrimWhitespace = draft.DynamicTrimWhitespace,
                        FailIfEmpty = draft.DynamicFailIfEmpty,
                        FallbackMessage = draft.DynamicFallbackMessage
                    }
                }
                : null,
            CoreSettings = draft.Type == OfflineModuleType.CoreSettings && draft.CoreSettings != null
                ? new CoreSettingsBlock
                {
                    Enabled = draft.CoreSettings.Enabled,
                    PollingIntervalSeconds = draft.CoreSettings.PollingIntervalSeconds,
                    AutoClearModules = draft.CoreSettings.AutoClearModules,
                    SoundEnabled = draft.CoreSettings.SoundEnabled,
                    ExitMenuVisible = draft.CoreSettings.ExitMenuVisible,
                    StartStopMenuVisible = draft.CoreSettings.StartStopMenuVisible,
                    HeartbeatSeconds = draft.CoreSettings.HeartbeatSeconds
                }
                : null
        };

        return JsonSerializer.Serialize(manifest, _jsonOptions);
    }

    private static string MapType(OfflineModuleType type) => type switch
    {
        OfflineModuleType.Standard => "standard",
        OfflineModuleType.Conditional => "conditional",
        OfflineModuleType.Dynamic => "dynamic",
        OfflineModuleType.Hero => "hero",
        OfflineModuleType.CoreSettings => "core_update",
        _ => "standard"
    };

    private sealed class ManifestModel
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = "standard";
        public string Category { get; set; } = "General";
        public string? Title { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime? ScheduleUtc { get; set; }
        public DateTime? ExpiresUtc { get; set; }
        public MediaBlock? Media { get; set; }
        public BehaviorBlock? Behavior { get; set; }
        public DynamicBlock? Dynamic { get; set; }
        public CoreSettingsBlock? CoreSettings { get; set; }
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
        public bool TrimWhitespace { get; set; }
        public bool FailIfEmpty { get; set; }
        public string? FallbackMessage { get; set; }
    }

    private sealed class CoreSettingsBlock
    {
        public int Enabled { get; set; }
        public int PollingIntervalSeconds { get; set; }
        public int AutoClearModules { get; set; }
        public int SoundEnabled { get; set; }
        public int ExitMenuVisible { get; set; }
        public int StartStopMenuVisible { get; set; }
        public int HeartbeatSeconds { get; set; }
    }
}
