using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace DeepResearch.DurableFunctions;

public class DeepResearchStarter(ILogger<DeepResearchStarter> logger)
{
    [Function(nameof(DeepResearchStarter))]
    public async Task<HttpResponseData> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        var body = await JsonSerializer.DeserializeAsync<DeepResearchOrchestratorArguments>(req.Body, JsonSerializerOptions.Web);
        if (body is null || string.IsNullOrWhiteSpace(body.Topic))
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await errorResponse.WriteStringAsync("Invalid request body. 'Topic' is required.");
            return errorResponse;
        }

        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
            nameof(DeepResearchOrchestrator),
            body);

        logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);
        return await client.CreateCheckStatusResponseAsync(req, instanceId);
    }
}
