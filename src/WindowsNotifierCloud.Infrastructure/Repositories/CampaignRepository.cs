using Microsoft.EntityFrameworkCore;
using WindowsNotifierCloud.Domain.Entities;
using WindowsNotifierCloud.Domain.Interfaces;
using WindowsNotifierCloud.Infrastructure.Persistence;

namespace WindowsNotifierCloud.Infrastructure.Repositories;

public class CampaignRepository : ICampaignRepository
{
    private readonly ApplicationDbContext _db;

    public CampaignRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Campaign?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Campaigns
            .Include(c => c.Modules)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Campaign>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Campaigns
            .Include(c => c.Modules)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Campaign campaign, CancellationToken cancellationToken = default)
    {
        await _db.Campaigns.AddAsync(campaign, cancellationToken);
    }

    public void Delete(Campaign campaign)
    {
        _db.Campaigns.Remove(campaign);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }
}
