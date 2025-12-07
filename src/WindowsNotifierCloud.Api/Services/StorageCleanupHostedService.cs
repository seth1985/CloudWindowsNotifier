using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WindowsNotifierCloud.Api.Services;

public class StorageCleanupHostedService : BackgroundService
{
    private readonly ILogger<StorageCleanupHostedService> _logger;
    private readonly StorageCleanupService _cleanup;
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);

    public StorageCleanupHostedService(ILogger<StorageCleanupHostedService> logger, StorageCleanupService cleanup)
    {
        _logger = logger;
        _cleanup = cleanup;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // initial delay
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var removed = await _cleanup.RunRetentionAsync(stoppingToken);
                _logger.LogInformation("Storage cleanup completed. Removed {Removed} items.", removed);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Storage cleanup failed.");
            }

            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (TaskCanceledException) { }
        }
    }
}
