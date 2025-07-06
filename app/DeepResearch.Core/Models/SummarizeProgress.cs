namespace DeepResearch.Core.Models;

public class SummarizeProgress : ProgressBase
{
    public SummarizeProgress() : base(ProgressTypes.Summarize)
    {
    }

    public string Summary { get; set; } = string.Empty;
}