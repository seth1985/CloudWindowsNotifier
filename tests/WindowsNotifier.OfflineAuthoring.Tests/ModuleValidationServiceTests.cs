using WindowsNotifier.OfflineAuthoring.Core.Models;
using WindowsNotifier.OfflineAuthoring.Core.Services;

namespace WindowsNotifier.OfflineAuthoring.Tests;

public sealed class ModuleValidationServiceTests
{
    [Fact]
    public void Validate_Fails_WhenDynamicModuleHasNoScript()
    {
        var service = new ModuleValidationService();
        var draft = new OfflineModuleDraft
        {
            ModuleId = "module-dyn",
            Type = OfflineModuleType.Dynamic,
            Title = "Dynamic title",
            DynamicScriptBody = ""
        };

        var result = service.Validate(draft);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Dynamic script", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_Passes_ForBasicStandardModule()
    {
        var service = new ModuleValidationService();
        var draft = new OfflineModuleDraft
        {
            ModuleId = "module-standard",
            Type = OfflineModuleType.Standard,
            Title = "Title",
            Message = "Message"
        };

        var result = service.Validate(draft);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenLinkIsNotAbsoluteHttp()
    {
        var service = new ModuleValidationService();
        var draft = new OfflineModuleDraft
        {
            ModuleId = "module-link",
            Type = OfflineModuleType.Standard,
            Title = "Title",
            Message = "Message",
            LinkUrl = "not-a-valid-url"
        };

        var result = service.Validate(draft);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Link URL", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_Fails_WhenConditionalIntervalMissingForConditionalType()
    {
        var service = new ModuleValidationService();
        var draft = new OfflineModuleDraft
        {
            ModuleId = "module-conditional",
            Type = OfflineModuleType.Conditional,
            Title = "Conditional",
            Message = "Condition message",
            ConditionalScriptBody = "return $true",
            ConditionalIntervalMinutes = null
        };

        var result = service.Validate(draft);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Conditional interval", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_Fails_ForInvalidCoreSettingsFlags()
    {
        var service = new ModuleValidationService();
        var draft = new OfflineModuleDraft
        {
            ModuleId = "module-core",
            Type = OfflineModuleType.CoreSettings,
            CoreSettings = new CoreSettingsDraft
            {
                Enabled = 2,
                PollingIntervalSeconds = 300,
                AutoClearModules = 1,
                SoundEnabled = 1,
                ExitMenuVisible = 0,
                StartStopMenuVisible = 0,
                HeartbeatSeconds = 15
            }
        };

        var result = service.Validate(draft);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("0 or 1", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_Fails_WhenExpiryIsNotAfterSchedule()
    {
        var service = new ModuleValidationService();
        var now = DateTime.UtcNow;
        var draft = new OfflineModuleDraft
        {
            ModuleId = "module-schedule",
            Type = OfflineModuleType.Standard,
            Title = "Title",
            Message = "Message",
            ScheduleUtc = now.AddHours(2),
            ExpiresUtc = now.AddHours(1)
        };

        var result = service.Validate(draft);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Expiry time", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_Passes_ForValidCoreSettingsModule()
    {
        var service = new ModuleValidationService();
        var draft = new OfflineModuleDraft
        {
            ModuleId = "module-core",
            Type = OfflineModuleType.CoreSettings,
            CoreSettings = new CoreSettingsDraft
            {
                Enabled = 1,
                PollingIntervalSeconds = 300,
                AutoClearModules = 1,
                SoundEnabled = 1,
                ExitMenuVisible = 0,
                StartStopMenuVisible = 0,
                HeartbeatSeconds = 15
            }
        };

        var result = service.Validate(draft);

        Assert.True(result.IsValid);
    }
}
