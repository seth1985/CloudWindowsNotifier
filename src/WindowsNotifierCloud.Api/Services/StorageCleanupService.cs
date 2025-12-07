using Microsoft.Extensions.DependencyInjection;

namespace WindowsNotifierCloud.Api.Services;

public class StorageCleanupService
{
    private readonly StorageOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;

    public StorageCleanupService(StorageOptions options, IServiceScopeFactory scopeFactory)
    {
        _options = options;
        _scopeFactory = scopeFactory;
    }

    public string ExportsRoot => Path.Combine(_options.Root, "modules", "exports");
    public string AssetsRoot => Path.Combine(_options.Root, "modules", "assets");

    public void RemoveModuleArtifacts(Guid moduleDbId, string moduleId)
    {
        try
        {
            var exportFolder = Path.Combine(ExportsRoot, Sanitize(moduleId));
            if (Directory.Exists(exportFolder))
            {
                Directory.Delete(exportFolder, recursive: true);
            }

            var assetsFolder = Path.Combine(AssetsRoot, moduleDbId.ToString());
            if (Directory.Exists(assetsFolder))
            {
                Directory.Delete(assetsFolder, recursive: true);
            }

            var moduleAssetsFolder = Path.Combine(_options.Root, "module-assets", moduleDbId.ToString());
            if (Directory.Exists(moduleAssetsFolder))
            {
                Directory.Delete(moduleAssetsFolder, recursive: true);
            }
        }
        catch
        {
            // best-effort cleanup
        }
    }

    public async Task<int> RunRetentionAsync(CancellationToken ct = default)
    {
        EnsureRoot();
        var retention = _options.Retention;
        var removed = 0;

        // prune exports per module
        if (Directory.Exists(ExportsRoot))
        {
            foreach (var moduleDir in Directory.GetDirectories(ExportsRoot))
            {
                removed += PruneExportsForModule(moduleDir, retention);
            }
        }

        // prune old zips
        if (Directory.Exists(ExportsRoot))
        {
            removed += PruneOldZips(ExportsRoot, retention.MaxZipAgeDays);
        }

        // prune orphan assets
        if (retention.PruneOrphans && Directory.Exists(AssetsRoot))
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<WindowsNotifierCloud.Domain.Interfaces.IModuleRepository>();
            var dbModules = (await repo.ListAsync(ct)).Select(m => m.Id.ToString()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var assetDir in Directory.GetDirectories(AssetsRoot))
            {
                var name = Path.GetFileName(assetDir);
                if (!dbModules.Contains(name))
                {
                    Directory.Delete(assetDir, recursive: true);
                    removed++;
                }
            }
        }

        if (retention.PruneOrphans)
        {
            var moduleAssetsRoot = Path.Combine(_options.Root, "module-assets");
            if (Directory.Exists(moduleAssetsRoot))
            {
                using var scope = _scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<WindowsNotifierCloud.Domain.Interfaces.IModuleRepository>();
                var dbModules = (await repo.ListAsync(ct)).Select(m => m.Id.ToString()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                foreach (var assetDir in Directory.GetDirectories(moduleAssetsRoot))
                {
                    var name = Path.GetFileName(assetDir);
                    if (!dbModules.Contains(name))
                    {
                        Directory.Delete(assetDir, recursive: true);
                        removed++;
                    }
                }
            }
        }

        return removed;
    }

    private int PruneExportsForModule(string moduleDir, StorageRetentionOptions retention)
    {
        var removed = 0;
        var versionDirs = Directory.GetDirectories(moduleDir)
            .Select(d => new DirectoryInfo(d))
            .OrderByDescending(d => d.CreationTimeUtc)
            .ToList();

        if (retention.RetainExportVersions > 0 && versionDirs.Count > retention.RetainExportVersions)
        {
            foreach (var dir in versionDirs.Skip(retention.RetainExportVersions))
            {
                dir.Delete(true);
                removed++;
            }
        }

        if (retention.MaxExportAgeDays > 0)
        {
            var cutoff = DateTime.UtcNow.AddDays(-retention.MaxExportAgeDays);
            foreach (var dir in versionDirs.Where(d => d.CreationTimeUtc < cutoff))
            {
                if (dir.Exists)
                {
                    dir.Delete(true);
                    removed++;
                }
            }
        }

        return removed;
    }

    private static int PruneOldZips(string root, int maxAgeDays)
    {
        if (maxAgeDays <= 0) return 0;
        var cutoff = DateTime.UtcNow.AddDays(-maxAgeDays);
        var removed = 0;
        foreach (var zip in Directory.GetFiles(root, "*.zip", SearchOption.AllDirectories))
        {
            var info = new FileInfo(zip);
            if (info.CreationTimeUtc < cutoff)
            {
                info.Delete();
                removed++;
            }
        }
        return removed;
    }

    private void EnsureRoot()
    {
        if (string.IsNullOrWhiteSpace(_options.Root))
            throw new InvalidOperationException("Storage root not configured.");
        Directory.CreateDirectory(_options.Root);
        Directory.CreateDirectory(ExportsRoot);
        Directory.CreateDirectory(AssetsRoot);
    }

    private static string Sanitize(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "module";
        var invalid = Path.GetInvalidFileNameChars();
        return new string(name.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
    }
}
