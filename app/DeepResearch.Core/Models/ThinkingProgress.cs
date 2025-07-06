namespace DeepResearch.Core.Models;

public class ThinkingProgress : ProgressBase
{
    public ThinkingProgress() : base(ProgressTypes.Thinking)
    {
    }

    public string Message { get; set; } = string.Empty;
}