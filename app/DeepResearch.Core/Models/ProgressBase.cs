using System.Text.Json.Serialization;

namespace DeepResearch.Core.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(ThinkingProgress), ProgressTypes.Thinking)]
[JsonDerivedType(typeof(QueryGenerationProgress), ProgressTypes.GenerateQuery)]
[JsonDerivedType(typeof(WebResearchProgress), ProgressTypes.WebResearch)]
[JsonDerivedType(typeof(SummarizeProgress), ProgressTypes.Summarize)]
[JsonDerivedType(typeof(ReflectionProgress), ProgressTypes.Reflection)]
[JsonDerivedType(typeof(RoutingProgress), ProgressTypes.Routing)]
[JsonDerivedType(typeof(FinalizeProgress), ProgressTypes.Finalize)]
[JsonDerivedType(typeof(ResearchCompleteProgress), ProgressTypes.ResearchComplete)]
[JsonDerivedType(typeof(ErrorProgress), ProgressTypes.Error)]
public abstract class ProgressBase(string type)
{
    public string Type => type;
    
    // TimeProvider�͊eProgress�N���X�쐬���ɊO������ݒ肳���
    public DateTime Timestamp { get; set; }

    public string Step => Type;
}