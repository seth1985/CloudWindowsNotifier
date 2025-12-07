using System.Text;
using WindowsNotifierCloud.Domain.Entities;
using WindowsNotifierCloud.Domain.Interfaces;

namespace WindowsNotifierCloud.Api.Services;

public class ExportService
{
    private readonly IModuleRepository _modules;
    private readonly ManifestBuilder _manifestBuilder;
    private readonly StorageOptions _storage;

    public ExportService(IModuleRepository modules, ManifestBuilder manifestBuilder, StorageOptions storage)
    {
        _modules = modules;
        _manifestBuilder = manifestBuilder;
        _storage = storage;
    }

    public async Task<ExportResult?> ExportAsync(Guid moduleDbId, CancellationToken ct = default)
    {
        var module = await _modules.GetAsync(moduleDbId, ct);
        if (module == null) return null;

        EnsureStorageConfigured();

        // Use module.ModuleId as the folder name so exports are predictable and not overwritten by different ModuleId values.
        // Include version to keep historical exports side-by-side.
        var moduleFolderName = SanitizeFolderName(module.ModuleId);
        var versionFolder = $"version-{module.Version}";
        var exportRoot = Path.Combine(_storage.Root, "modules", "exports", moduleFolderName, versionFolder, "module");
        Directory.CreateDirectory(exportRoot);

        // Write manifest
        var manifestJson = _manifestBuilder.BuildManifest(module);
        await File.WriteAllTextAsync(Path.Combine(exportRoot, "manifest.json"), manifestJson, Encoding.UTF8, ct);

        // Write scripts from stored bodies (if present)
        if (module.Type == ModuleType.Conditional && !string.IsNullOrWhiteSpace(module.ConditionalScriptBody))
        {
            await File.WriteAllTextAsync(Path.Combine(exportRoot, "conditional.ps1"), module.ConditionalScriptBody, Encoding.UTF8, ct);
        }

        if (module.Type == ModuleType.Dynamic && !string.IsNullOrWhiteSpace(module.DynamicScriptBody))
        {
            await File.WriteAllTextAsync(Path.Combine(exportRoot, "dynamic.ps1"), module.DynamicScriptBody, Encoding.UTF8, ct);
        }

        string? warning = null;

        if (module.Type == ModuleType.Hero)
        {
            var heroPath = ResolveHeroPath(module.Id);
            if (heroPath == null)
            {
                throw new InvalidOperationException($"Hero image missing for module '{module.ModuleId}'. Upload hero.png before export.");
            }
            var destPath = Path.Combine(exportRoot, "hero.png");
            File.Copy(heroPath, destPath, overwrite: true);
        }
        else
        {
            // Copy icon if available; warn if none specified.
            if (!string.IsNullOrWhiteSpace(module.IconFileName))
            {
                var iconPath = ResolveIconPath(module.IconFileName, module.Id);
                if (iconPath == null)
                {
                    throw new InvalidOperationException($"Icon file '{module.IconFileName}' not found for module '{module.ModuleId}'.");
                }

                var destPath = Path.Combine(exportRoot, module.IconFileName);
                File.Copy(iconPath, destPath, overwrite: true);
            }
            else
            {
                warning = "No icon specified; export will proceed without an icon.";
            }
        }

        return new ExportResult(exportRoot, module.Version, moduleFolderName, null, null, warning);
    }

    public async Task<ExportResult?> ExportZipAsync(Guid moduleDbId, CancellationToken ct = default)
    {
        var export = await ExportAsync(moduleDbId, ct);
        if (export == null) return null;

        var zipName = $"{Path.GetFileName(export.Path)}.zip";
        var zipPath = Path.Combine(Path.GetDirectoryName(export.Path) ?? _storage.Root, zipName);

        if (File.Exists(zipPath))
        {
            File.Delete(zipPath);
        }

        System.IO.Compression.ZipFile.CreateFromDirectory(export.Path, zipPath);
        return export with { PackagePath = zipPath };
    }

    public async Task<ExportResult?> ExportToDevCoreAsync(Guid moduleDbId, CancellationToken ct = default)
    {
        var export = await ExportAsync(moduleDbId, ct);
        if (export == null) return null;

        if (string.IsNullOrWhiteSpace(_storage.DevCoreModulesRoot))
            throw new InvalidOperationException("DevCoreModulesRoot is not configured.");

        var destination = Path.Combine(_storage.DevCoreModulesRoot!, export.ModuleFolderName ?? "module");

        CopyDirectory(export.Path, destination, overwrite: true);
        return export with { DevCorePath = destination };
    }

    private void EnsureStorageConfigured()
    {
        if (string.IsNullOrWhiteSpace(_storage.Root))
            throw new InvalidOperationException("Storage.Root is not configured.");
        Directory.CreateDirectory(_storage.Root);
    }

    private string? ResolveIconPath(string fileName, Guid moduleId)
    {
        var pathsToCheck = new List<string>();

        // Current path
        pathsToCheck.Add(Path.Combine(_storage.Root, "modules", "assets", moduleId.ToString(), fileName));
        pathsToCheck.Add(Path.Combine(_storage.Root, "module-assets", moduleId.ToString(), fileName));

        // Legacy path (pre ApiStorage rename)
        if (_storage.Root.Contains("ApiStorage", StringComparison.OrdinalIgnoreCase))
        {
            var legacyRoot = _storage.Root.Replace("ApiStorage", "Storage");
            pathsToCheck.Add(Path.Combine(legacyRoot, "modules", "assets", moduleId.ToString(), fileName));
            pathsToCheck.Add(Path.Combine(legacyRoot, "module-assets", moduleId.ToString(), fileName));
        }

        foreach (var p in pathsToCheck)
        {
            if (File.Exists(p))
            {
                return p;
            }
        }

        return null;
    }

    private string? ResolveHeroPath(Guid moduleId)
    {
        var pathsToCheck = new List<string>
        {
            Path.Combine(_storage.Root, "module-assets", moduleId.ToString(), "hero.png"),
            Path.Combine(_storage.Root, "modules", "assets", moduleId.ToString(), "hero.png")
        };

        if (_storage.Root.Contains("ApiStorage", StringComparison.OrdinalIgnoreCase))
        {
            var legacyRoot = _storage.Root.Replace("ApiStorage", "Storage");
            pathsToCheck.Add(Path.Combine(legacyRoot, "module-assets", moduleId.ToString(), "hero.png"));
            pathsToCheck.Add(Path.Combine(legacyRoot, "modules", "assets", moduleId.ToString(), "hero.png"));
        }

        foreach (var p in pathsToCheck)
        {
            if (File.Exists(p)) return p;
        }
        return null;
    }

    private static string SanitizeFolderName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return $"module-{Guid.NewGuid():N}";
        }

        var invalid = Path.GetInvalidFileNameChars();
        var safe = new string(name.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
        return safe;
    }

private static void CopyDirectory(string sourceDir, string destDir, bool overwrite)
    {
        if (Directory.Exists(destDir) && overwrite)
        {
            Directory.Delete(destDir, recursive: true);
        }
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var dest = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, dest, overwrite: true);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var dest = Path.Combine(destDir, Path.GetFileName(dir));
            CopyDirectory(dir, dest, overwrite);
        }
    }
}

public record ExportResult(string Path, int Version, string? ModuleFolderName, string? DevCorePath = null, string? PackagePath = null, string? Warning = null);
