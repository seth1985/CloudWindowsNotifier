using WindowsNotifier.OfflineAuthoring.Core.Models;

namespace WindowsNotifier.OfflineAuthoring.Core.Abstractions;

public interface IOfflineTemplateStore
{
    Task<IReadOnlyList<OfflineScriptTemplate>> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(IReadOnlyList<OfflineScriptTemplate> templates, CancellationToken cancellationToken = default);
}
