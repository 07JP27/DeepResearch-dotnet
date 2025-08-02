using DeepResearch.Core;
using LongRunningDeepResearch.ChatClient;
using LongRunningDeepResearch.SearchClient;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace LongRunningDeepResearch;

public static class DeepResearchOrchestrator
{
    [Function(nameof(DeepResearchOrchestrator))]
    public static async Task<ResearchResult> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(DeepResearchOrchestrator));

        var topic = context.GetInput<string>();
        ArgumentException.ThrowIfNullOrEmpty(topic);

        var deepResearchService = new DeepResearchService(
            new OrchestratorChatClient(context),
            new OrchestratorSearchClient(context));
        return await deepResearchService.RunResearchAsync(topic, 
            new DeepResearchOptions
            {
                MaxResearchLoops = 3,
            }));
    }
}
