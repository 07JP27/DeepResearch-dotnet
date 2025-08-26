using DeepResearch.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DeepResearch.DurableFunctions;

public class DeepResearchStarter(ILogger<DeepResearchStarter> logger)
{
    [Function(nameof(DeepResearchStarter))]
    public async Task<IResult> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        var body = await JsonSerializer.DeserializeAsync<DeepResearchOrchestratorArguments>(req.Body, JsonSerializerOptions.Web);
        if (body is null || string.IsNullOrWhiteSpace(body.Topic))
        {
            return Results.BadRequest("Invalid request body. 'Topic' is required.");
        }

        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
            nameof(DeepResearchOrchestrator),
            body);

        logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);
        var payload = client.CreateHttpManagementPayload(instanceId);
        return Results.Ok(payload);
    }
}
