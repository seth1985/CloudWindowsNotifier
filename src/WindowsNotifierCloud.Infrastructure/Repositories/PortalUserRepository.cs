using Microsoft.EntityFrameworkCore;
using WindowsNotifierCloud.Domain.Entities;
using WindowsNotifierCloud.Domain.Interfaces;
using WindowsNotifierCloud.Infrastructure.Persistence;

namespace WindowsNotifierCloud.Infrastructure.Repositories;

public class PortalUserRepository : IPortalUserRepository
{
    private readonly ApplicationDbContext _db;

    public PortalUserRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<PortalUser?> GetByLocalUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return _db.PortalUsers.FirstOrDefaultAsync(u => u.LocalUsername == username, cancellationToken);
    }

    public async Task AddAsync(PortalUser user, CancellationToken cancellationToken = default)
    {
        await _db.PortalUsers.AddAsync(user, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }
}
