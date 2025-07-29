using DeepResearch.Core.SearchClient;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;

namespace LongRunningDeepResearch.SearchClient;
public class SearchActivity([FromKeyedServices(SearchActivity.SearchClientKey)]ISearchClient searchClient)
{
    public const string SearchClientKey = $"{nameof(SearchActivity)}_{nameof(ISearchClient)}";

    [Function(nameof(SearchActivity))]
    public async Task<SearchResult> SearchAsync(
        [ActivityTrigger]SearchArguments args, 
        CancellationToken cancellationToken = default) => 
        await searchClient.SearchAsync(args.Query, args.MaxResults, cancellationToken);
}
