using WindowsNotifierCloud.Domain.Entities;

namespace WindowsNotifierCloud.Domain.Interfaces;

public interface ICampaignRepository
{
    Task<Campaign?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Campaign>> ListAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Campaign campaign, CancellationToken cancellationToken = default);
    void Delete(Campaign campaign);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
