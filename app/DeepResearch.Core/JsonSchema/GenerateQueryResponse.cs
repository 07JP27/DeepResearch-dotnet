using System.ComponentModel;


namespace DeepResearch.Core;


internal class GenerateQueryResponse
{
    [Description("検索クエリ")]
    public string Query { get; set; } = string.Empty;

    [Description("なぜその検索を行う必要があるのかを論理的に説明してください。")]
    public string Rationale { get; set; } = string.Empty;
}
