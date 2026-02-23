using WindowsNotifier.OfflineAuthoring.Core.Models;

namespace WindowsNotifier.OfflineAuthoring.Core.Services;

public sealed class ModuleValidationService
{
    public ModuleValidationResult Validate(OfflineModuleDraft draft)
    {
        var result = new ModuleValidationResult();
        var invalidFileNameChars = Path.GetInvalidFileNameChars();

        if (string.IsNullOrWhiteSpace(draft.ModuleId))
        {
            result.AddError("ModuleId", "Module ID is required.");
        }
        else if (draft.ModuleId.Any(ch => invalidFileNameChars.Contains(ch)))
        {
            result.AddError("ModuleId", "Module ID contains invalid file name characters.");
        }

        if (draft.Type != OfflineModuleType.CoreSettings && string.IsNullOrWhiteSpace(draft.Title))
        {
            result.AddError("Title", "Title is required for non-CoreSettings modules.");
        }

        if (!string.IsNullOrWhiteSpace(draft.LinkUrl))
        {
            if (!Uri.TryCreate(draft.LinkUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                result.AddError("LinkUrl", "Link URL must be an absolute http/https URL.");
            }
        }

        if ((draft.Type == OfflineModuleType.Standard || draft.Type == OfflineModuleType.Conditional)
            && string.IsNullOrWhiteSpace(draft.Message))
        {
            result.AddError("Message", "Message is required for Standard and Conditional modules.");
        }

        if (draft.ReminderHours.HasValue && draft.ReminderHours.Value < 0)
        {
            result.AddError("ReminderHours", "Reminder hours cannot be negative.");
        }

        if (draft.ScheduleUtc.HasValue && draft.ExpiresUtc.HasValue && draft.ExpiresUtc.Value <= draft.ScheduleUtc.Value)
        {
            result.AddError("ExpiresUtc", "Expiry time must be later than the scheduled start time.");
        }

        if (draft.Type == OfflineModuleType.Conditional && string.IsNullOrWhiteSpace(draft.ConditionalScriptBody))
        {
            result.AddError("ConditionalScriptBody", "Conditional script is required for Conditional modules.");
        }
        if (draft.Type == OfflineModuleType.Conditional &&
            (!draft.ConditionalIntervalMinutes.HasValue || draft.ConditionalIntervalMinutes.Value <= 0))
        {
            result.AddError("ConditionalIntervalMinutes", "Conditional interval minutes must be greater than zero for Conditional modules.");
        }

        if (draft.Type == OfflineModuleType.Dynamic && string.IsNullOrWhiteSpace(draft.DynamicScriptBody))
        {
            result.AddError("DynamicScriptBody", "Dynamic script is required for Dynamic modules.");
        }
        if (draft.Type == OfflineModuleType.Dynamic && draft.DynamicMaxLength <= 0)
        {
            result.AddError("DynamicMaxLength", "Dynamic max length must be greater than zero.");
        }

        if (draft.Type == OfflineModuleType.Hero)
        {
            if (string.IsNullOrWhiteSpace(draft.HeroSourcePath))
            {
                result.AddError("HeroSourcePath", "Hero source image path is required for Hero modules.");
            }
            else if (!File.Exists(draft.HeroSourcePath))
            {
                result.AddError("HeroSourcePath", "Hero source image file was not found.");
            }
        }

        if (!string.IsNullOrWhiteSpace(draft.IconSourcePath) && !File.Exists(draft.IconSourcePath))
        {
            result.AddError("IconSourcePath", "Icon source file was not found.");
        }
        else if (!string.IsNullOrWhiteSpace(draft.IconSourcePath))
        {
            var ext = Path.GetExtension(draft.IconSourcePath)?.ToLowerInvariant();
            if (ext is not (".png" or ".jpg" or ".jpeg" or ".ico"))
            {
                result.AddError("IconSourcePath", "Icon source must be .png, .jpg, .jpeg, or .ico.");
            }

            var size = new FileInfo(draft.IconSourcePath).Length;
            if (size > 512_000)
            {
                result.AddError("IconSourcePath", "Icon source exceeds 512 KB limit.");
            }
        }

        if (!string.IsNullOrWhiteSpace(draft.HeroSourcePath) && File.Exists(draft.HeroSourcePath))
        {
            var ext = Path.GetExtension(draft.HeroSourcePath)?.ToLowerInvariant();
            if (ext != ".png")
            {
                result.AddError("HeroSourcePath", "Hero source must be .png.");
            }

            var size = new FileInfo(draft.HeroSourcePath).Length;
            if (size > 1_024_000)
            {
                result.AddError("HeroSourcePath", "Hero source exceeds 1 MB limit.");
            }
        }

        if (draft.Type == OfflineModuleType.CoreSettings)
        {
            if (draft.CoreSettings == null)
            {
                result.AddError("CoreSettings", "Core settings payload is required for CoreSettings modules.");
            }
            else
            {
                if (draft.CoreSettings.PollingIntervalSeconds <= 0)
                {
                    result.AddError("CorePollingIntervalSeconds", "Core polling interval must be greater than zero.");
                }
                if (draft.CoreSettings.HeartbeatSeconds <= 0)
                {
                    result.AddError("CoreHeartbeatSeconds", "Core heartbeat seconds must be greater than zero.");
                }

                ValidateBinaryFlag(draft.CoreSettings.Enabled, "CoreEnabled", "Core enabled", result);
                ValidateBinaryFlag(draft.CoreSettings.AutoClearModules, "CoreAutoClearModules", "Core auto clear modules", result);
                ValidateBinaryFlag(draft.CoreSettings.SoundEnabled, "CoreSoundEnabled", "Core sound enabled", result);
                ValidateBinaryFlag(draft.CoreSettings.ExitMenuVisible, "CoreExitMenuVisible", "Core exit menu visible", result);
                ValidateBinaryFlag(draft.CoreSettings.StartStopMenuVisible, "CoreStartStopMenuVisible", "Core start/stop menu visible", result);
            }
        }

        return result;
    }

    private static void ValidateBinaryFlag(int value, string fieldName, string displayName, ModuleValidationResult result)
    {
        if (value is not (0 or 1))
        {
            result.AddError(fieldName, $"{displayName} must be 0 or 1.");
        }
    }
}
