using System.Text.Json;
using WindowsNotifier.OfflineAuthoring.Core.Abstractions;
using WindowsNotifier.OfflineAuthoring.Core.Models;

namespace WindowsNotifier.OfflineAuthoring.Infrastructure.Persistence;

public sealed class JsonOfflineProjectStore : IOfflineProjectStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    public async Task SaveAsync(string filePath, OfflineModuleDraft draft, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(draft);
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path is required.", nameof(filePath));
        }

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(draft, SerializerOptions);
        await File.WriteAllTextAsync(filePath, json, cancellationToken);
    }

    public async Task<OfflineModuleDraft> LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path is required.", nameof(filePath));
        }
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Project file was not found.", filePath);
        }

        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        var draft = JsonSerializer.Deserialize<OfflineModuleDraft>(json, SerializerOptions);
        if (draft == null)
        {
            throw new InvalidDataException("Project file is invalid or empty.");
        }

        return draft;
    }
}
