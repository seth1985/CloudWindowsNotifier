using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace WindowsNotifierCloud.Api.Services;

public class StorageService
{
    private readonly StorageOptions _options;

    public StorageService(StorageOptions options)
    {
        _options = options;
    }

    public string GetAssetsFolder(Guid moduleId)
    {
        EnsureRoot();
        var path = Path.Combine(_options.Root, "modules", "assets", moduleId.ToString());
        Directory.CreateDirectory(path);
        return path;
    }

    public async Task<StoredAsset> SaveAssetAsync(Guid moduleId, IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0) throw new InvalidOperationException("Empty file.");

        EnsureRoot();
        var assetsFolder = GetAssetsFolder(moduleId);

        var safeName = SanitizeFileName(file.FileName);
        var destPath = Path.Combine(assetsFolder, safeName);

        await using var stream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(stream, cancellationToken);

        return new StoredAsset(safeName, file.FileName, destPath);
    }

    private void EnsureRoot()
    {
        if (string.IsNullOrWhiteSpace(_options.Root))
            throw new InvalidOperationException("Storage root not configured.");
        Directory.CreateDirectory(_options.Root);
    }

    private static string SanitizeFileName(string name)
    {
        var sanitized = Regex.Replace(name ?? string.Empty, @"[^A-Za-z0-9\.\-_]", "_");
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            sanitized = $"file_{Guid.NewGuid():N}";
        }
        return sanitized;
    }
}

public record StoredAsset(string FileName, string OriginalName, string FullPath);
