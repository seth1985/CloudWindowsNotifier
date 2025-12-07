using WindowsNotifierCloud.Domain.Entities;

namespace WindowsNotifierCloud.Domain.Interfaces;

public interface IPortalUserRepository
{
    Task<PortalUser?> GetByLocalUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task AddAsync(PortalUser user, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
