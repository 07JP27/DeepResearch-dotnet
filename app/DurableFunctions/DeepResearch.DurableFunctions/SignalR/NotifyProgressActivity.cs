using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.SignalRService;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DeepResearch.DurableFunctions.SignalR;

[SignalRConnection]
public class NotifyProgressActivity(IServiceProvider serviceProvider, ILogger<NotifyProgressActivity> logger) : ServerlessHub(serviceProvider)
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
        logger.LogInformation("Notifying user {UserId} with progress: {Progress}", 
            args.SessionId, 
            args.Progress.Progress.Type);
        try
        {
            await Clients.User(args.SessionId).SendAsync("progress", JsonSerializer.Serialize(args.Progress, JsonSerializerOptions.Web));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error notifying user {UserId} with progress: {Progress}", 
                args.SessionId, 
                args.Progress.Progress.Type);
            throw;
        }
    }
}
