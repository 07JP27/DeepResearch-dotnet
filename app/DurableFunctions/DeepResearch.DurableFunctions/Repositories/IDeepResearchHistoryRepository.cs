using DeepResearch.Core.Models;

namespace DeepResearch.DurableFunctions.Repositories;
public interface IDeepResearchHistoryRepository
{
    ValueTask<ProgressBase[]> GetProgressesBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
    ValueTask AddProgressAsync(string sessionId, string progressId, ProgressBase progress, CancellationToken cancellationToken = default);
    ValueTask<ProgressBase> GetProgressAsync(string sessionId, string progressId, CancellationToken cancellationToken = default);
}
