using System.Text.Json;
using WindowsNotifier.OfflineAuthoring.Core.Abstractions;
using WindowsNotifier.OfflineAuthoring.Core.Models;

namespace WindowsNotifier.OfflineAuthoring.Infrastructure.Persistence;

public sealed class JsonOfflineTemplateStore : IOfflineTemplateStore
{
    private readonly string _templatesFilePath;
    private readonly JsonSerializerOptions _serializerOptions;

    public JsonOfflineTemplateStore(string? templatesFilePath = null)
    {
        _templatesFilePath = templatesFilePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Windows Notifier",
            "OfflineTemplates",
            "templates.json");

        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    public async Task<IReadOnlyList<OfflineScriptTemplate>> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_templatesFilePath))
        {
            var defaults = BuildDefaultTemplates();
            await SaveAsync(defaults, cancellationToken);
            return defaults;
        }

        await using var stream = File.OpenRead(_templatesFilePath);
        var templates = await JsonSerializer.DeserializeAsync<List<OfflineScriptTemplate>>(
            stream,
            _serializerOptions,
            cancellationToken);

        return templates ?? new List<OfflineScriptTemplate>();
    }

    public async Task SaveAsync(IReadOnlyList<OfflineScriptTemplate> templates, CancellationToken cancellationToken = default)
    {
        var parent = Path.GetDirectoryName(_templatesFilePath);
        if (!string.IsNullOrWhiteSpace(parent))
        {
            Directory.CreateDirectory(parent);
        }

        await using var stream = File.Create(_templatesFilePath);
        await JsonSerializer.SerializeAsync(stream, templates, _serializerOptions, cancellationToken);
    }

    private static IReadOnlyList<OfflineScriptTemplate> BuildDefaultTemplates() =>
        new List<OfflineScriptTemplate>
        {
            new()
            {
                Name = "Device Online Check",
                Type = OfflineScriptTemplateType.Conditional,
                ScriptBody =
@"$online = Test-Connection -ComputerName 8.8.8.8 -Count 1 -Quiet
return $online"
            },
            new()
            {
                Name = "Top CPU Process",
                Type = OfflineScriptTemplateType.Dynamic,
                ScriptBody =
@"$process = Get-Process | Sort-Object CPU -Descending | Select-Object -First 1
if ($null -eq $process) { return $null }
return ""Top CPU: $($process.ProcessName) ($([math]::Round($process.CPU, 1)))"""
            }
        };
}
