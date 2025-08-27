using DeepResearch.DurableFunctions.Adapters.Activities;
using Microsoft.DurableTask;
using Microsoft.Extensions.AI;

namespace DeepResearch.DurableFunctions.Adapters;

public class DurableChatClient(TaskOrchestrationContext context) : IChatClient
{
    private static TaskOptions DefaultTaskOptions => new()
    {
        Retry = new RetryPolicy(5, TimeSpan.FromSeconds(1), 1, TimeSpan.FromSeconds(60))
    };

    public void Dispose() { }

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default) => 
        await context.CallActivityAsync<ChatResponse>(
            DurableChatClientActivity.GetResponseFunctionName,
            new GetResponseArguments(messages, options),
            DefaultTaskOptions);

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, 
        ChatOptions? options = null, 
        CancellationToken cancellationToken = default) => 
        throw new NotSupportedException("Streaming responses are not supported in DurableChatClient. Use GetResponseAsync instead.");
}
