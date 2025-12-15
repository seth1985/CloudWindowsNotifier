using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WindowsNotifierTray;

internal sealed class TelemetryClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl;
    private readonly string _apiKey;

    public TelemetryClient(string apiUrl, string apiKey)
    {
        _apiUrl = apiUrl;
        _apiKey = apiKey;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    public async Task SendAsync(string moduleId, string eventType, Dictionary<string, object>? additional = null)
    {
        if (string.IsNullOrWhiteSpace(_apiUrl) || string.IsNullOrWhiteSpace(_apiKey))
        {
            return;
        }

        Dictionary<string, object?> payload = new()
        {
            ["moduleId"] = moduleId,
            ["deviceId"] = Environment.MachineName,
            ["userPrincipalName"] = $"{Environment.UserDomainName}\\{Environment.UserName}",
            ["eventType"] = eventType,
            ["occurredAtUtc"] = DateTime.UtcNow.ToString("o")
        };

        if (additional != null && additional.Count > 0)
        {
            payload["additionalData"] = additional;
        }

        try
        {
            var json = JsonSerializer.Serialize(payload);
            using var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl);
            request.Headers.Add("x-wn-api-key", _apiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                QueueManager.Enqueue(new TelemetryQueueItem
                {
                    Id = Guid.NewGuid(),
                    CreatedUtc = DateTime.UtcNow,
                    Attempts = 0,
                    Payload = payload
                });
            }
        }
        catch
        {
            QueueManager.Enqueue(new TelemetryQueueItem
            {
                Id = Guid.NewGuid(),
                CreatedUtc = DateTime.UtcNow,
                Attempts = 0,
                Payload = payload
            });
        }
    }

    public async Task RetryPendingAsync()
    {
        await QueueManager.RetryAsync(async (item) =>
        {
            try
            {
                var json = JsonSerializer.Serialize(item.Payload);
                using var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl);
                request.Headers.Add("x-wn-api-key", _apiKey);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        });
    }
}

internal sealed class TelemetryQueueItem
{
    public Guid Id { get; set; }
    public DateTime CreatedUtc { get; set; }
    public int Attempts { get; set; }
    public DateTime? LastAttemptUtc { get; set; }
    public Dictionary<string, object?> Payload { get; set; } = new();
}

/// <summary>
/// Persistent JSONL queue for telemetry retries. Events are appended on failure,
/// retried on a timer with exponential backoff, and dropped after max attempts
/// or age/size limits.
/// </summary>
internal static class QueueManager
{
    private static readonly object Sync = new();
    private static readonly string QueuePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WindowsNotifier", "Telemetry", "pending.jsonl");

    private const int MaxAttempts = 10;
    private static readonly TimeSpan MaxAge = TimeSpan.FromDays(7);
    private const long MaxFileBytes = 5 * 1024 * 1024; // 5 MB

    public static void Enqueue(TelemetryQueueItem item)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(QueuePath)!);
            lock (Sync)
            {
                File.AppendAllText(QueuePath, JsonSerializer.Serialize(item) + Environment.NewLine);
                TrimIfNeeded();
            }
        }
        catch { }
    }

    public static async Task RetryAsync(Func<TelemetryQueueItem, Task<bool>> sender)
    {
        List<TelemetryQueueItem> items;
        lock (Sync)
        {
            items = ReadAll();
        }

        if (items.Count == 0) return;

        var now = DateTime.UtcNow;
        var survivors = new List<TelemetryQueueItem>();

        foreach (var item in items)
        {
            if (item.Attempts >= MaxAttempts) continue;
            if (now - item.CreatedUtc > MaxAge) continue;

            // Exponential backoff: 5min * 2^attempts, capped at 60min
            var delayMinutes = Math.Min(60, (int)(5 * Math.Pow(2, Math.Max(0, item.Attempts))));
            var baseTime = item.LastAttemptUtc ?? item.CreatedUtc;
            var nextDue = baseTime.AddMinutes(delayMinutes);
            if (now < nextDue)
            {
                survivors.Add(item);
                continue;
            }

            var ok = await sender(item);
            if (!ok)
            {
                item.Attempts += 1;
                item.LastAttemptUtc = now;
                survivors.Add(item);
            }
        }

        lock (Sync)
        {
            WriteAll(survivors);
        }
    }

    private static List<TelemetryQueueItem> ReadAll()
    {
        if (!File.Exists(QueuePath)) return new List<TelemetryQueueItem>();
        var list = new List<TelemetryQueueItem>();
        foreach (var line in File.ReadAllLines(QueuePath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            try
            {
                var item = JsonSerializer.Deserialize<TelemetryQueueItem>(line);
                if (item != null) list.Add(item);
            }
            catch { }
        }
        return list;
    }

    private static void WriteAll(List<TelemetryQueueItem> items)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(QueuePath)!);
        var lines = items.Select(i => JsonSerializer.Serialize(i));
        File.WriteAllLines(QueuePath, lines);
        TrimIfNeeded();
    }

    private static void TrimIfNeeded()
    {
        try
        {
            var info = new FileInfo(QueuePath);
            if (!info.Exists) return;

            var lines = File.ReadAllLines(QueuePath).ToList();
            if (lines.Count == 0) return;

            long SizeOf(List<string> content) =>
                Encoding.UTF8.GetByteCount(string.Join(Environment.NewLine, content) + Environment.NewLine);

            while (lines.Count > 0 && SizeOf(lines) > MaxFileBytes)
            {
                lines.RemoveAt(0); // drop oldest
            }

            File.WriteAllLines(QueuePath, lines);
        }
        catch { }
    }
}
