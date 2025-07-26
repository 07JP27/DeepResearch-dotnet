namespace DeepResearch.Core.Models;

public class ThinkingProgress() : ProgressBase(ProgressTypes.Thinking)
{
    public string Message { get; set; } = string.Empty;
}