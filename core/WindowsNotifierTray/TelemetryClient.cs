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

        try
        {
            var payload = new Dictionary<string, object?>
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

            var json = JsonSerializer.Serialize(payload);
            using var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl);
            request.Headers.Add("x-wn-api-key", _apiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            await _httpClient.SendAsync(request);
        }
        catch
        {
            // best effort; ignore errors
        }
    }
}
