using DeepResearch.Core;
using DeepResearch.Core.Models;
using LongRunningDeepResearch.ChatClient;
using LongRunningDeepResearch.SearchClient;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
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
        if (string.IsNullOrWhiteSpace(topic))
        {
            throw new ArgumentException("Topic cannot be null or empty.", nameof(topic));
        }

        var deepResearchSearvice = new DeepResearchService(
            new OrchestratorChatClient(context),
            new OrchestratorSearchClient(context));
        List<ProgressBase> progressReports = new List<ProgressBase>();
        return await deepResearchSearvice.RunResearchAsync(topic, 
            new DeepResearchOptions
            {
                MaxResearchLoops = 3,
            }, 
            new Progress<ProgressBase>(progress =>
            {
                progressReports.Add(progress);
            }));
    }
}
