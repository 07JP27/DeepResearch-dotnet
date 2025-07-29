using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace LongRunningDeepResearch.ChatClient;
public class GetResponseActivity([FromKeyedServices(GetResponseActivity.ChatClientKey)]IChatClient chatClient)
{
    public const string ChatClientKey = $"{nameof(GetResponseActivity)}_{nameof(ChatClientKey)}";

    [Function(nameof(GetResponseActivity))]
    public async Task<ChatResponse> GetResponseAsync(
        [ActivityTrigger]GetResponseArguments args, 
        CancellationToken cancellationToken = default) => 
        await chatClient.GetResponseAsync(args.Messages, args.Options, cancellationToken);
}
