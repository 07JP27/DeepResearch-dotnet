using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace DeepResearch.DurableFunctions.Adapters.Activities;

public class DurableChatClientActivity([FromKeyedServices(DurableChatClientActivity.ChatClientKey)] IChatClient chatClient)
{
    public const string ChatClientKey = $"{nameof(DurableChatClientActivity)}_ChatClient";
    public const string GetResponseFunctionName = $"{nameof(DurableChatClientActivity)}_{nameof(GetResponse)}";

    [Function(GetResponseFunctionName)]
    public async Task<ChatResponse> GetResponse(
        [ActivityTrigger] GetResponseArguments arguments,
        CancellationToken cancellationToken) =>
        await chatClient.GetResponseAsync(arguments.Messages, arguments.Options, cancellationToken);
}

public record GetResponseArguments(IEnumerable<ChatMessage> Messages, ChatOptions? Options);