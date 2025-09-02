using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.SignalRService;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using DeepResearch.DurableFunctions.Repositories;

namespace DeepResearch.DurableFunctions.SignalR;

[SignalRConnection]
public class NotifyProgressActivity(IServiceProvider serviceProvider, 
    IDeepResearchHistoryRepository historyRepository,
    ILogger<NotifyProgressActivity> logger) : ServerlessHub(serviceProvider)
{
    [Function("negotiate")]
    public async Task<HttpResponseData> Negotiate(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        var sessionId = req.Query["sessionId"];
        if (string.IsNullOrEmpty(sessionId))
        {
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteStringAsync("Session ID is required.");
            return response;
        }

        var negotiateResponse = await NegotiateAsync(new() { UserId = sessionId });
        var responseData = req.CreateResponse(HttpStatusCode.OK);
        await responseData.WriteBytesAsync(negotiateResponse.ToArray());
        return responseData;
    }

    [Function(nameof(NotifyProgressActivity))]
    public async Task Run([ActivityTrigger] NotifyProgressArguments args)
    {
        logger.LogInformation("Notifying user {ProgressKey} with progress: {Progress}", 
            args.ProgressKey, 
            args.Progress.Progress.Type);
        try
        {
            await historyRepository.AddProgressAsync(args.ProgressKey, args.Progress.Progress);
            await Clients.User(args.ProgressKey.SessionId).SendAsync("progress", args.ProgressKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error notifying user {ProgressKey} with progress: {Progress}", 
                args.ProgressKey, 
                args.Progress.Progress.Type);
            throw;
        }
    }
}
