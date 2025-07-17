namespace DeepResearch.Core.Models;

public class SummarizeProgress() : ProgressBase(ProgressTypes.Summarize)
{
    public string Summary { get; set; } = string.Empty;
}