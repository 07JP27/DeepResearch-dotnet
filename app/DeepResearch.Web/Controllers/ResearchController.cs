using Microsoft.AspNetCore.Mvc;
using DeepResearch.Web.Services;

namespace DeepResearch.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResearchController : ControllerBase
{
    private readonly ResearchProgressService _progressService;
    private readonly WebResearchService _researchService;
    private readonly ILogger<ResearchController> _logger;

    public ResearchController(
        ResearchProgressService progressService,
        WebResearchService researchService,
        ILogger<ResearchController> logger)
    {
        _progressService = progressService;
        _researchService = researchService;
        _logger = logger;
    }

    [HttpGet("progress/{clientId}")]
    public IActionResult GetProgress(string clientId, [FromQuery] int fromIndex = 0)
    {
        try
        {
            var progress = _progressService.GetProgress(clientId, fromIndex);
            var isResearching = _progressService.IsResearching(clientId);

            return Ok(new
            {
                progress = progress,
                isResearching = isResearching,
                totalCount = _progressService.GetProgressCount(clientId)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "進行状況の取得中にエラーが発生しました");
            return StatusCode(500, new { error = "進行状況の取得に失敗しました" });
        }
    }

    [HttpPost("start")]
    public IActionResult StartResearch([FromBody] StartResearchRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Topic) || string.IsNullOrWhiteSpace(request.ClientId))
            {
                return BadRequest(new { error = "トピックとクライアントIDは必須です" });
            }

            if (_progressService.IsResearching(request.ClientId))
            {
                return Conflict(new { error = "すでに調査が実行中です" });
            }

            _progressService.ClearProgress(request.ClientId);
            _progressService.SetResearchStatus(request.ClientId, true);

            // バックグラウンドで調査を開始
            _ = Task.Run(async () =>
            {
                try
                {
                    await _researchService.StartResearchAsync(request.Topic, request.ClientId);
                }
                finally
                {
                    _progressService.SetResearchStatus(request.ClientId, false);
                }
            });

            return Ok(new { message = "調査を開始しました" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "調査の開始中にエラーが発生しました");
            _progressService.SetResearchStatus(request.ClientId, false);
            return StatusCode(500, new { error = "調査の開始に失敗しました" });
        }
    }

    [HttpDelete("progress/{clientId}")]
    public IActionResult ClearProgress(string clientId)
    {
        try
        {
            _progressService.ClearProgress(clientId);
            return Ok(new { message = "進行状況をクリアしました" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "進行状況のクリア中にエラーが発生しました");
            return StatusCode(500, new { error = "進行状況のクリアに失敗しました" });
        }
    }
}

public class StartResearchRequest
{
    public string Topic { get; set; } = "";
    public string ClientId { get; set; } = "";
}
