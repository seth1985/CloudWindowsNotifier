using System.Text.Json;
using WindowsNotifier.OfflineAuthoring.Core.Models;
using WindowsNotifier.OfflineAuthoring.Core.Services;

namespace WindowsNotifier.OfflineAuthoring.Tests;

public sealed class ManifestGenerationServiceTests
{
    [Fact]
    public void BuildManifestJson_ForStandardModule_ContainsExpectedFields()
    {
        var service = new ManifestGenerationService();
        var draft = new OfflineModuleDraft
        {
            ModuleId = "module-test-standard",
            Type = OfflineModuleType.Standard,
            Category = "General",
            Title = "Title",
            Message = "Body",
            IconFileName = "info.png"
        };

        var json = service.BuildManifestJson(draft);
        using var doc = JsonDocument.Parse(json);

        Assert.Equal("module-test-standard", doc.RootElement.GetProperty("id").GetString());
        Assert.Equal("standard", doc.RootElement.GetProperty("type").GetString());
        Assert.Equal("info.png", doc.RootElement.GetProperty("media").GetProperty("icon").GetString());
        Assert.False(doc.RootElement.TryGetProperty("dynamic", out _));
    }

    [Fact]
    public void BuildManifestJson_ForDynamicModule_ContainsDynamicSection()
    {
        var service = new ManifestGenerationService();
        var draft = new OfflineModuleDraft
        {
            ModuleId = "module-test-dynamic",
            Type = OfflineModuleType.Dynamic,
            Title = "Dynamic title",
            DynamicScriptBody = "$output='hi'",
            DynamicMaxLength = 160,
            DynamicTrimWhitespace = true,
            DynamicFailIfEmpty = true
        };

        var json = service.BuildManifestJson(draft);
        using var doc = JsonDocument.Parse(json);
        var dynamicBlock = doc.RootElement.GetProperty("dynamic");

        Assert.Equal("dynamic", doc.RootElement.GetProperty("type").GetString());
        Assert.Equal("dynamic.ps1", dynamicBlock.GetProperty("script").GetString());
        Assert.Equal(160, dynamicBlock.GetProperty("max_length").GetInt32());
    }

    [Fact]
    public void BuildManifestJson_Throws_WhenModuleIdMissing()
    {
        var service = new ManifestGenerationService();
        var draft = new OfflineModuleDraft { ModuleId = "" };

        Assert.Throws<InvalidOperationException>(() => service.BuildManifestJson(draft));
    }

    [Fact]
    public void BuildManifestJson_ForCoreSettings_ContainsCoreSettingsBlock()
    {
        var service = new ManifestGenerationService();
        var draft = new OfflineModuleDraft
        {
            ModuleId = "module-core",
            Type = OfflineModuleType.CoreSettings,
            CoreSettings = new CoreSettingsDraft
            {
                Enabled = 1,
                PollingIntervalSeconds = 120,
                AutoClearModules = 1,
                SoundEnabled = 0,
                ExitMenuVisible = 1,
                StartStopMenuVisible = 1,
                HeartbeatSeconds = 10
            }
        };

        var json = service.BuildManifestJson(draft);
        using var doc = JsonDocument.Parse(json);

        Assert.Equal("core_update", doc.RootElement.GetProperty("type").GetString());
        var core = doc.RootElement.GetProperty("core_settings");
        Assert.Equal(120, core.GetProperty("polling_interval_seconds").GetInt32());
        Assert.Equal(10, core.GetProperty("heartbeat_seconds").GetInt32());
    }
}
