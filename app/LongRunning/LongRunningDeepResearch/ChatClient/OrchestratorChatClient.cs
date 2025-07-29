using Microsoft.DurableTask;
using Microsoft.Extensions.AI;

namespace LongRunningDeepResearch.ChatClient;
public class OrchestratorChatClient(TaskOrchestrationContext taskOrchestrationContext) : IChatClient
{
    public void Dispose() { }

    public async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        return await taskOrchestrationContext.CallActivityAsync<ChatResponse>(
            nameof(GetResponseActivity),
            new GetResponseArguments(messages, options));
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        if (serviceType == typeof(IChatClient)) return this;
        return null;
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}

public record GetResponseArguments(IEnumerable<ChatMessage> Messages, ChatOptions? Options);
