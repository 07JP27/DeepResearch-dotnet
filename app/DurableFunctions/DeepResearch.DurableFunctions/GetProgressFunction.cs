using DeepResearch.Core.Models;
using DeepResearch.DurableFunctions.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace DeepResearch.DurableFunctions;
public class GetProgressFunction(IDeepResearchHistoryRepository historyRepository)
{
    [Function("GetProgress")]
    public async Task<IActionResult> GetProgressAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "progress/{sessionId}/{progressId}")] HttpRequest request,
        string sessionId,
        string progressId,
        CancellationToken cancellationToken)
    {
        var progress = await historyRepository.GetProgressAsync(new(sessionId, progressId), cancellationToken);
        if (progress is null)
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(new ProgressEnvelope(progress));
    }

    [Function("GetAllProgress")]
    public async Task<IActionResult> GetAllProgressAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "progress/{sessionId}")] HttpRequest request,
        string sessionId,
        CancellationToken cancellationToken)
    {
        var progresses = await historyRepository.GetProgressesBySessionIdAsync(sessionId, cancellationToken);
        return new OkObjectResult(progresses.Select(p => new ProgressEnvelope(p)));
    }
}
