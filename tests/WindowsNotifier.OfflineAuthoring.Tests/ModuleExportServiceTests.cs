using WindowsNotifier.OfflineAuthoring.Core.Models;
using WindowsNotifier.OfflineAuthoring.Core.Services;

namespace WindowsNotifier.OfflineAuthoring.Tests;

public sealed class ModuleExportServiceTests
{
    [Fact]
    public async Task ExportModuleAsync_WritesManifestAndDynamicScript()
    {
        var exportService = new ModuleExportService(new ManifestGenerationService());
        var draft = new OfflineModuleDraft
        {
            ModuleId = "module-export-dynamic",
            Type = OfflineModuleType.Dynamic,
            Title = "Dynamic",
            DynamicScriptBody = "Write-Output 'hello'"
        };

        var tempRoot = Path.Combine(Path.GetTempPath(), $"wn-export-{Guid.NewGuid():N}");
        try
        {
            var output = await exportService.ExportModuleAsync(draft, tempRoot);

            Assert.True(File.Exists(Path.Combine(output, "manifest.json")));
            Assert.True(File.Exists(Path.Combine(output, "dynamic.ps1")));
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }
}
