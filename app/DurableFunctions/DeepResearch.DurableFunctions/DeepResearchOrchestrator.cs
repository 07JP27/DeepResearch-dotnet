using DeepResearch.Core;
using DeepResearch.Core.Models;
using DeepResearch.DurableFunctions.Adapters;
using DeepResearch.DurableFunctions.SignalR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DeepResearch.DurableFunctions;

public static class DeepResearchOrchestrator
{
    private static readonly TaskOptions DefaultTaskOptions = new()
    {
        Retry = new RetryPolicy(5, TimeSpan.FromSeconds(1), 1, TimeSpan.FromSeconds(60))
    };

    [Function(nameof(DeepResearchOrchestrator))]
    public static async Task<ResearchResult> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(DeepResearchOrchestrator));

        var args = context.GetInput<DeepResearchOrchestratorArguments>();
        ArgumentNullException.ThrowIfNull(args);
        ArgumentException.ThrowIfNullOrEmpty(args.Topic);

        var deepResearchService = new DeepResearchService(
            new DurableChatClient(context),
            new DurableSearchClient(context),
            new DurableTimeProvider(context));
        var progress = new AsyncProgress<ProgressBase>(async (update, _) =>
        {
            await context.CallActivityAsync(
                nameof(NotifyProgressActivity),
                new NotifyProgressArguments(args.SessionId, new(update)),
                DefaultTaskOptions);
        });
        return await deepResearchService.RunResearchAsync(args.Topic,
            new DeepResearchOptions
            {
                MaxResearchLoops = args.MaxResearchLoops,
            },
            progress);
    }
}