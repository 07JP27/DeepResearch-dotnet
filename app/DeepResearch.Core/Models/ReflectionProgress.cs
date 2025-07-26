namespace DeepResearch.Core.Models;

public class ReflectionProgress() : ProgressBase(ProgressTypes.Reflection)
{
    public string Query { get; set; } = string.Empty;
    public string KnowledgeGap { get; set; } = string.Empty;
}