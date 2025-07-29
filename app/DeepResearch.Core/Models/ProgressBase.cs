using System.Text.Json.Serialization;

namespace DeepResearch.Core.Models;

[JsonDerivedType(typeof(ErrorProgress))]
[JsonDerivedType(typeof(FinalizeProgress))]
[JsonDerivedType(typeof(QueryGenerationProgress))]
[JsonDerivedType(typeof(ReflectionProgress))]
[JsonDerivedType(typeof(ResearchCompleteProgress))]
[JsonDerivedType(typeof(RoutingProgress))]
[JsonDerivedType(typeof(SummarizeProgress))]
[JsonDerivedType(typeof(ThinkingProgress))]
[JsonDerivedType(typeof(WebResearchProgress))]
public abstract class ProgressBase(string type)
{
    public string Type => type;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string Step => Type;
}