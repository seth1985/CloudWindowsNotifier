using WindowsNotifier.OfflineAuthoring.Core.Models;
using WindowsNotifier.OfflineAuthoring.Infrastructure.Persistence;

namespace WindowsNotifier.OfflineAuthoring.Tests;

public sealed class JsonOfflineProjectStoreTests
{
    [Fact]
    public async Task SaveAndLoadAsync_RoundTripsDraft()
    {
        var store = new JsonOfflineProjectStore();
        var draft = new OfflineModuleDraft
        {
            ModuleId = "module-roundtrip",
            Type = OfflineModuleType.Conditional,
            Title = "Patch Required",
            Message = "Install updates",
            ConditionalIntervalMinutes = 30,
            ConditionalScriptBody = "exit 0"
        };

        var tempPath = Path.Combine(Path.GetTempPath(), $"wn-offline-{Guid.NewGuid():N}.wnproj");
        try
        {
            await store.SaveAsync(tempPath, draft);
            var loaded = await store.LoadAsync(tempPath);

            Assert.Equal(draft.ModuleId, loaded.ModuleId);
            Assert.Equal(draft.Type, loaded.Type);
            Assert.Equal(draft.ConditionalIntervalMinutes, loaded.ConditionalIntervalMinutes);
            Assert.Equal(draft.ConditionalScriptBody, loaded.ConditionalScriptBody);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}
