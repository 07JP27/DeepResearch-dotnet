namespace DeepResearch.Core.Models;

public class ResearchCompleteProgress : ProgressBase
{
    public ResearchCompleteProgress() : base(ProgressTypes.ResearchComplete)
    {
    }

    public string Status { get; set; } = string.Empty;
    public string? FinalSummary { get; set; }
    public List<string> Images { get; set; } = new();
}