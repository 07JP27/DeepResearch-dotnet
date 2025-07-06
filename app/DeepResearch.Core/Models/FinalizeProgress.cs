namespace DeepResearch.Core.Models;

public class FinalizeProgress : ProgressBase
{
    public FinalizeProgress() : base(ProgressTypes.Finalize)
    {
    }

    public string Summary { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new();
}