using DeepResearch.Core.SearchClient;
using DeepResearch.DurableFunctions.Adapters.Activities;
using Microsoft.DurableTask;

namespace DeepResearch.DurableFunctions.Adapters;
public class DurableSearchClient(TaskOrchestrationContext context) : ISearchClient
{
    private static TaskOptions DefaultTaskOptions => new()
    {
        Retry = new RetryPolicy(5, TimeSpan.FromSeconds(1), 1, TimeSpan.FromSeconds(60))
    };

    public async Task<SearchResult> SearchAsync(string query, int maxResults, CancellationToken cancellationToken = default) => 
        await context.CallActivityAsync<SearchResult>(
            DurableSearchClientActivity.SearchFunctionName,
            new SearchArguments(query, maxResults),
            DefaultTaskOptions);
}
