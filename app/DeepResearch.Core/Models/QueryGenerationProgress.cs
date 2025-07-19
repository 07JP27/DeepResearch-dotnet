namespace DeepResearch.Core.Models;

public class QueryGenerationProgress() : ProgressBase(ProgressTypes.GenerateQuery)
{
    public string Query { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
}