using WindowsNotifier.OfflineAuthoring.Core.Models;
using WindowsNotifier.OfflineAuthoring.Infrastructure.Persistence;

namespace WindowsNotifier.OfflineAuthoring.Tests;

public sealed class JsonOfflineTemplateStoreTests
{
    [Fact]
    public async Task Load_WhenMissingFile_SeedsDefaults()
    {
        var root = Path.Combine(Path.GetTempPath(), "wn-offline-templates-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, "templates.json");
        var store = new JsonOfflineTemplateStore(path);

        var loaded = await store.LoadAsync();

        Assert.NotEmpty(loaded);
        Assert.True(File.Exists(path));
        Assert.Contains(loaded, t => t.Type == OfflineScriptTemplateType.Conditional);
        Assert.Contains(loaded, t => t.Type == OfflineScriptTemplateType.Dynamic);
    }

    [Fact]
    public async Task SaveAndLoad_PersistsTemplates()
    {
        var root = Path.Combine(Path.GetTempPath(), "wn-offline-templates-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, "templates.json");
        var store = new JsonOfflineTemplateStore(path);
        var templates = new List<OfflineScriptTemplate>
        {
            new()
            {
                Name = "Check Defender",
                Type = OfflineScriptTemplateType.Conditional,
                ScriptBody = "return $true"
            }
        };

        await store.SaveAsync(templates);
        var loaded = await store.LoadAsync();

        Assert.Single(loaded);
        Assert.Equal("Check Defender", loaded[0].Name);
        Assert.Equal(OfflineScriptTemplateType.Conditional, loaded[0].Type);
        Assert.Equal("return $true", loaded[0].ScriptBody);
    }
}
