namespace DeepResearch.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DeepResearch.Core.Clients;

public static class Formatting
{
    /// <summary>
    /// 検索APIのレスポンスを重複排除し、構造化された文字列に整形します。
    /// </summary>
    /// <param name="searchResponse">'results'キーを持つ辞書、またはそのリスト</param>
    /// <param name="maxTokensPerSource">各ソースの最大トークン数</param>
    /// <param name="fetchFullPage">全文を含めるかどうか</param>
    /// <returns>重複排除・整形済み文字列</returns>
    // SearchResult型を受け取るオーバーロード
    public static string DeduplicateAndFormatSources(
        SearchResult searchResult,
        int maxTokensPerSource,
        bool fetchFullPage = false)
    {
        if (searchResult == null || searchResult.Results == null)
            return string.Empty;

        // URLで重複排除
        var uniqueSources = new Dictionary<string, SearchResultItem>();
        foreach (var source in searchResult.Results)
        {
            if (!string.IsNullOrEmpty(source.Url) && !uniqueSources.ContainsKey(source.Url))
            {
                uniqueSources[source.Url] = source;
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine("Sources:\n");
        foreach (var source in uniqueSources.Values)
        {
            string title = !string.IsNullOrEmpty(source.Title) ? source.Title : "(no title)";
            string url = !string.IsNullOrEmpty(source.Url) ? source.Url : "(no url)";
            string content = !string.IsNullOrEmpty(source.Content) ? source.Content : "";
            sb.AppendLine($"Source: {title}\n===");
            sb.AppendLine($"URL: {url}\n===");
            sb.AppendLine($"Most relevant content from source: {content}\n===");
            if (fetchFullPage)
            {
                int charLimit = maxTokensPerSource * 4;
                string rawContent = !string.IsNullOrEmpty(source.RawContent) ? source.RawContent : "";
                if (rawContent.Length > charLimit)
                {
                    rawContent = rawContent.Substring(0, charLimit) + "... [truncated]";
                }
                sb.AppendLine($"Full source content limited to {maxTokensPerSource} tokens: {rawContent}\n");
            }
        }
        return sb.ToString().Trim();
    }

    /// <summary>
    /// 検索結果をタイトルとURLの箇条書きリストに整形します。
    /// </summary>
    /// <param name="searchResults">'results'キーを持つ検索レスポンス</param>
    /// <returns>"* title : url"形式の文字列</returns>
    // SearchResult型を受け取るオーバーロード
    public static string FormatSources(SearchResult searchResult)
    {
        if (searchResult == null || searchResult.Results == null)
            return string.Empty;
        var lines = new List<string>();
        foreach (var item in searchResult.Results)
        {
            string title = !string.IsNullOrEmpty(item.Title) ? item.Title : "(no title)";
            string url = !string.IsNullOrEmpty(item.Url) ? item.Url : "(no url)";
            lines.Add($"* {title} : {url}");
        }
        return string.Join("\n", lines);
    }
}

