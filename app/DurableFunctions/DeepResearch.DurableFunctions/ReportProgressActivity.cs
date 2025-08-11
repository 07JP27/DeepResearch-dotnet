using DeepResearch.Core.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DeepResearch.DurableFunctions;

public class ReportProgressActivity(ILogger<ReportProgressActivity> logger)
{
    [Function(nameof(ReportProgressActivity))]
    public Task ReportProgressAsync(
        [ActivityTrigger] ProgressEnvelope progressEnvelope)
    {
        logger.LogInformation("Progress update: {progress}",
            JsonSerializer.Serialize(progressEnvelope.Progress, JsonSerializerOptions.Web));
        return Task.CompletedTask;
    }
}
