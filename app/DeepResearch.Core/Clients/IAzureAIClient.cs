namespace DeepResearch.Core.Clients;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class Message
{
    public string Role { get; set; } // "system", "user", "assistant"
    public string Content { get; set; }
}

public interface IAzureAIClient
{
    Task<string> GetCompletionAsync(List<Message> messages, CancellationToken cancellationToken = default);
}
