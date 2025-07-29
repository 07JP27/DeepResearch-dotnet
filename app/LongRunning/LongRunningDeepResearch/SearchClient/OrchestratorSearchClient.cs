using DeepResearch.Core.SearchClient;
using Microsoft.DurableTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningDeepResearch.SearchClient;
public class OrchestratorSearchClient(TaskOrchestrationContext taskOrchestrationContext) : ISearchClient
{
    public async Task<SearchResult> SearchAsync(string query, int maxResults, CancellationToken cancellationToken = default) => 
        await taskOrchestrationContext.CallActivityAsync<SearchResult>(
            nameof(SearchActivity),
            new SearchArguments(query, maxResults));
}

public record SearchArguments(string Query, int MaxResults);
