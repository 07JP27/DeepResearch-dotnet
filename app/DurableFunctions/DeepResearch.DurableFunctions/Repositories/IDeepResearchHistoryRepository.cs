using DeepResearch.Core.Models;

namespace DeepResearch.DurableFunctions.Repositories;
public interface IDeepResearchHistoryRepository
{
    ValueTask<ProgressBase[]> GetProgressesBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
    ValueTask AddProgressAsync(ProgressKey progressKey, ProgressBase progress, CancellationToken cancellationToken = default);
    ValueTask<ProgressBase?> GetProgressAsync(ProgressKey progressKey, CancellationToken cancellationToken = default);
}
