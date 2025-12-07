using WindowsNotifierCloud.Domain.Entities;
using WindowsNotifierCloud.Domain.Interfaces;
using WindowsNotifierCloud.Infrastructure.Persistence;

namespace WindowsNotifierCloud.Infrastructure.Repositories;

public class TelemetryRepository : ITelemetryRepository
{
    private readonly ApplicationDbContext _db;

    public TelemetryRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task AddAsync(TelemetryEvent evt, CancellationToken cancellationToken = default)
    {
        return _db.TelemetryEvents.AddAsync(evt, cancellationToken).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }
}
