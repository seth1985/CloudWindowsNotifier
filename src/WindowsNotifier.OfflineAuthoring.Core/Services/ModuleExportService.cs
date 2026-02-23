using System.Text;
using WindowsNotifier.OfflineAuthoring.Core.Models;

namespace WindowsNotifier.OfflineAuthoring.Core.Services;

public sealed class ModuleExportService
{
    private readonly ManifestGenerationService _manifestService;

    public ModuleExportService(ManifestGenerationService manifestService)
    {
        _manifestService = manifestService;
    }

    public async Task<string> ExportModuleAsync(OfflineModuleDraft draft, string destinationRoot, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(destinationRoot))
        {
            throw new ArgumentException("Destination root is required.", nameof(destinationRoot));
        }

        Directory.CreateDirectory(destinationRoot);
        var folderName = SanitizeFolderName(draft.ModuleId);
        var moduleDirectory = Path.Combine(destinationRoot, folderName);

        if (Directory.Exists(moduleDirectory))
        {
            Directory.Delete(moduleDirectory, recursive: true);
        }
        Directory.CreateDirectory(moduleDirectory);

        var manifest = _manifestService.BuildManifestJson(draft);
        await File.WriteAllTextAsync(Path.Combine(moduleDirectory, "manifest.json"), manifest, Encoding.UTF8, cancellationToken);

        if (draft.Type == OfflineModuleType.Conditional && !string.IsNullOrWhiteSpace(draft.ConditionalScriptBody))
        {
            await File.WriteAllTextAsync(
                Path.Combine(moduleDirectory, "conditional.ps1"),
                draft.ConditionalScriptBody,
                Encoding.UTF8,
                cancellationToken);
        }

        if (draft.Type == OfflineModuleType.Dynamic && !string.IsNullOrWhiteSpace(draft.DynamicScriptBody))
        {
            await File.WriteAllTextAsync(
                Path.Combine(moduleDirectory, "dynamic.ps1"),
                draft.DynamicScriptBody,
                Encoding.UTF8,
                cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(draft.IconSourcePath) && File.Exists(draft.IconSourcePath))
        {
            var iconFileName = string.IsNullOrWhiteSpace(draft.IconFileName)
                ? Path.GetFileName(draft.IconSourcePath)
                : draft.IconFileName;
            File.Copy(draft.IconSourcePath, Path.Combine(moduleDirectory, iconFileName!), overwrite: true);
        }

        if (!string.IsNullOrWhiteSpace(draft.HeroSourcePath) && File.Exists(draft.HeroSourcePath))
        {
            var heroFileName = string.IsNullOrWhiteSpace(draft.HeroFileName) ? "hero.png" : draft.HeroFileName;
            File.Copy(draft.HeroSourcePath, Path.Combine(moduleDirectory, heroFileName!), overwrite: true);
        }

        return moduleDirectory;
    }

    private static string SanitizeFolderName(string moduleId)
    {
        var raw = string.IsNullOrWhiteSpace(moduleId) ? $"module-{Guid.NewGuid():N}" : moduleId;
        var invalid = Path.GetInvalidFileNameChars();
        return new string(raw.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
    }
}
