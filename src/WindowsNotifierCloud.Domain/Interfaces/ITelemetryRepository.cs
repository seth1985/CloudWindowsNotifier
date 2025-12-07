using WindowsNotifierCloud.Domain.Entities;

namespace WindowsNotifierCloud.Domain.Interfaces;

public interface ITelemetryRepository
{
    Task AddAsync(TelemetryEvent evt, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
