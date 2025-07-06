using System.ComponentModel;

namespace DeepResearch.Core.JsonSchema;

public class ReflectionOnSummaryResponse
{
    [Description("追加の検索クエリ")]
    public string FollowUpQuery { get; set; } = string.Empty;

    [Description("検索の知識ギャップ")]
    public string KnowledgeGap { get; set; } = string.Empty;
}
