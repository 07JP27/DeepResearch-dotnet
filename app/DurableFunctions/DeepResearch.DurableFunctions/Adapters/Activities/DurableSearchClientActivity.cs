using DeepResearch.Core.SearchClient;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;

namespace DeepResearch.DurableFunctions.Adapters.Activities;

public class DurableSearchClientActivity(
    [FromKeyedServices(DurableSearchClientActivity.SearchClientKey)]ISearchClient searchClient)
{
    public const string SearchClientKey = $"{nameof(DurableSearchClientActivity)}_SearchClient";
    public const string SearchFunctionName = $"{nameof(DurableSearchClientActivity)}_{nameof(Search)}";
    [Function(SearchFunctionName)]
    public async Task<SearchResult> Search(
        [ActivityTrigger] SearchArguments arguments,
        CancellationToken cancellationToken) =>
        await searchClient.SearchAsync(arguments.Query, arguments.MaxResults, cancellationToken);
}

public record SearchArguments(string Query, int MaxResults);