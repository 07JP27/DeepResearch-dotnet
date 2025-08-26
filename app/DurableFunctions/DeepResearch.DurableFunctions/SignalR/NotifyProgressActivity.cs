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
        var userId = req.Query["userId"];
        if (string.IsNullOrEmpty(userId))
        {
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteStringAsync("User ID is required.");
            return response;
        }

        var negotiateResponse = await NegotiateAsync(new() { UserId = userId });
        var responseData = req.CreateResponse(HttpStatusCode.OK);
        await responseData.WriteBytesAsync(negotiateResponse.ToArray());
        return responseData;
    }

    [Function(nameof(NotifyProgressActivity))]
    public async Task Run([ActivityTrigger] NotifyProgressArguments args)
    {
        logger.LogInformation("Notifying user {UserId} with progress: {Progress}", 
            args.UserId, 
            args.Progress.Progress.Type);
        try
        {
            await Clients.User(args.UserId).SendAsync("progress", JsonSerializer.Serialize(args.Progress, JsonSerializerOptions.Web));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error notifying user {UserId} with progress: {Progress}", 
                args.UserId, 
                args.Progress.Progress.Type);
            throw;
        }
    }
}
