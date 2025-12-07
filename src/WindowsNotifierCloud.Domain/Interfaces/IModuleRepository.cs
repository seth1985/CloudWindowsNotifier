using WindowsNotifierCloud.Domain.Entities;

namespace WindowsNotifierCloud.Domain.Interfaces;

public interface IModuleRepository
{
    Task<ModuleDefinition?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ModuleDefinition?> GetByModuleIdAsync(string moduleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ModuleDefinition>> ListAsync(CancellationToken cancellationToken = default);
    Task AddAsync(ModuleDefinition module, CancellationToken cancellationToken = default);
    void Delete(ModuleDefinition module);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
