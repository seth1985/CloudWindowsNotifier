using WindowsNotifier.OfflineAuthoring.Core.Models;

namespace WindowsNotifier.OfflineAuthoring.Core.Abstractions;

public interface IOfflineProjectStore
{
    Task SaveAsync(string filePath, OfflineModuleDraft draft, CancellationToken cancellationToken = default);
    Task<OfflineModuleDraft> LoadAsync(string filePath, CancellationToken cancellationToken = default);
}
