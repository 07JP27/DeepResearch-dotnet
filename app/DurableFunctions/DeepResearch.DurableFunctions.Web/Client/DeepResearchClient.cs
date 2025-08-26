using DeepResearch.Core.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Channels;

namespace DeepResearch.DurableFunctions.Web.Client;

public class DeepResearchClient(HttpClient httpClient, IConfiguration configuration)
{
    public async IAsyncEnumerable<ProgressBase> StartDeepResearchAsync(
        string sessionId,
        string topic,
        int maxResearchLoops = 3,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(sessionId);
        ArgumentException.ThrowIfNullOrEmpty(topic);

        // Create a channel to adapt SignalR push callbacks to IAsyncEnumerable
        var channel = Channel.CreateUnbounded<ProgressEnvelope>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });

        HubConnection? connection = null;
        IDisposable? progressSubscription = null;

        try
        {
            // 1) Negotiate to get the service URL and access token (relative URL against BaseAddress)
            // 2) Build the SignalR connection using negotiated info
            var realBaseAddress = ResolveRealBaseAddress();
            connection = new HubConnectionBuilder()
                .WithUrl(new Uri(new(realBaseAddress), $"api?sessionId={WebUtility.UrlEncode(sessionId)}"))
                .WithAutomaticReconnect()
                .Build();

            // Complete channel when the connection is closed and won't reconnect
            connection.Closed += async (ex) =>
            {
                if (ex is not null)
                {
                    channel.Writer.TryComplete(ex);
                }
                else
                {
                    channel.Writer.TryComplete();
                }
                await Task.CompletedTask;
            };

            // 3) Subscribe to progress messages
            progressSubscription = connection.On("progress", async (string jsonPayload) =>
            {
                try
                {
                    var envelope = JsonSerializer.Deserialize<ProgressEnvelope>(jsonPayload, JsonSerializerOptions.Web)
                        ?? throw new InvalidOperationException($"Invalid Json payload: {jsonPayload}");
                    // Forward to channel and complete on terminal message
                    if (envelope.Progress is ResearchCompleteProgress)
                    {
                        // Write the terminal message first so consumers see it
                        await channel.Writer.WriteAsync(envelope, cancellationToken);
                        channel.Writer.TryComplete();
                    }
                    else
                    {
                        await channel.Writer.WriteAsync(envelope, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    channel.Writer.TryComplete(ex);
                }
            });

            // 4) Start the connection before starting the orchestrator to avoid missing early events
            await connection.StartAsync(cancellationToken);

            // 5) Kick off the orchestrator (relative URL)
            var response = await httpClient.PostAsJsonAsync(
                "api/DeepResearchStarter",
                new DeepResearchOrchestratorArguments(sessionId, topic, maxResearchLoops),
                JsonSerializerOptions.Web,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                channel.Writer.TryComplete(new InvalidOperationException($"Failed to start deep research. StatusCode={response.StatusCode}"));
            }

            // 6) Yield items as they arrive
            await foreach (var item in channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return item.Progress;
            }
        }
        finally
        {
            // Ensure cleanup
            progressSubscription?.Dispose();
            if (connection is not null)
            {
                try { await connection.StopAsync(CancellationToken.None); } catch { /* ignore */ }
                await connection.DisposeAsync();
            }
        }
    }

    private string ResolveRealBaseAddress()
    {
        if (httpClient.BaseAddress == null)
        {
            throw new InvalidOperationException("HttpClient BaseAddress must be set before resolving the real base address.");
        }

        var schemas = httpClient.BaseAddress.Scheme.Split('+');
        foreach (var schema in schemas)
        {
            var realBaseAddress = configuration[$"services:{httpClient.BaseAddress.Host}:{schema}:0"];
            if (realBaseAddress != null)
            {
                return realBaseAddress;
            }
        }

        throw new InvalidOperationException($"Real base address not found for {httpClient.BaseAddress}.");
    }
}
