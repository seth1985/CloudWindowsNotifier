using Microsoft.EntityFrameworkCore;
using WindowsNotifierCloud.Domain.Entities;
using WindowsNotifierCloud.Domain.Interfaces;
using WindowsNotifierCloud.Infrastructure.Persistence;

namespace WindowsNotifierCloud.Infrastructure.Repositories;

public class ModuleRepository : IModuleRepository
{
    private readonly ApplicationDbContext _db;

    public ModuleRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ModuleDefinition?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.ModuleDefinitions
            .Include(m => m.Campaign)
            .Include(m => m.CreatedBy)
            .Include(m => m.LastModifiedBy)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<ModuleDefinition?> GetByModuleIdAsync(string moduleId, CancellationToken cancellationToken = default)
    {
        return await _db.ModuleDefinitions
            .Include(m => m.Campaign)
            .Include(m => m.CreatedBy)
            .Include(m => m.LastModifiedBy)
            .FirstOrDefaultAsync(m => m.ModuleId == moduleId, cancellationToken);
    }

    public async Task<IReadOnlyList<ModuleDefinition>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _db.ModuleDefinitions
            .Include(m => m.Campaign)
            .OrderBy(m => m.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ModuleDefinition module, CancellationToken cancellationToken = default)
    {
        await _db.ModuleDefinitions.AddAsync(module, cancellationToken);
    }

    public void Delete(ModuleDefinition module)
    {
        _db.ModuleDefinitions.Remove(module);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }
}
