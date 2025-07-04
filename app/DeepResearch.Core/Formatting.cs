namespace DeepResearch.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class Formatting
{
    /// <summary>
    /// 検索APIのレスポンスを重複排除し、構造化された文字列に整形します。
    /// </summary>
    /// <param name="searchResponse">'results'キーを持つ辞書、またはそのリスト</param>
    /// <param name="maxTokensPerSource">各ソースの最大トークン数</param>
    /// <param name="fetchFullPage">全文を含めるかどうか</param>
    /// <returns>重複排除・整形済み文字列</returns>
    public static string DeduplicateAndFormatSources(
        object searchResponse,
        int maxTokensPerSource,
        bool fetchFullPage = false)
    {
        // sources_listの構築
        List<Dictionary<string, object>> sourcesList = new();
        if (searchResponse is Dictionary<string, object> dict && dict.ContainsKey("results"))
        {
            if (dict["results"] is IEnumerable<object> results)
            {
                foreach (var item in results)
                {
                    if (item is Dictionary<string, object> src)
                        sourcesList.Add(src);
                }
            }
        }
        else if (searchResponse is IEnumerable<object> list)
        {
            foreach (var response in list)
            {
                if (response is Dictionary<string, object> d && d.ContainsKey("results"))
                {
                    if (d["results"] is IEnumerable<object> results)
                    {
                        foreach (var item in results)
                        {
                            if (item is Dictionary<string, object> src)
                                sourcesList.Add(src);
                        }
                    }
                }
                else if (response is IEnumerable<object> sublist)
                {
                    foreach (var item in sublist)
                    {
                        if (item is Dictionary<string, object> src)
                            sourcesList.Add(src);
                    }
                }
            }
        }
        else
        {
            throw new ArgumentException("Input must be either a dict with 'results' or a list of search results");
        }

        // URLで重複排除
        var uniqueSources = new Dictionary<string, Dictionary<string, object>>();
        foreach (var source in sourcesList)
        {
            if (source.TryGetValue("url", out var urlObj) && urlObj is string url)
            {
                if (!uniqueSources.ContainsKey(url))
                {
                    uniqueSources[url] = source;
                }
            }
        }

        // 整形
        var sb = new StringBuilder();
        sb.AppendLine("Sources:\n");
        int i = 1;
        foreach (var source in uniqueSources.Values)
        {
            string title = source.TryGetValue("title", out var t) && t is string ts ? ts : "(no title)";
            string url = source.TryGetValue("url", out var u) && u is string us ? us : "(no url)";
            string content = source.TryGetValue("content", out var c) && c is string cs ? cs : "";
            sb.AppendLine($"Source: {title}\n===");
            sb.AppendLine($"URL: {url}\n===");
            sb.AppendLine($"Most relevant content from source: {content}\n===");
            if (fetchFullPage)
            {
                int charLimit = maxTokensPerSource * 4;
                string rawContent = source.TryGetValue("raw_content", out var rc) && rc is string rcs ? rcs : "";
                if (rawContent == null)
                {
                    rawContent = "";
                    Console.WriteLine($"Warning: No raw_content found for source {url}");
                }
                if (rawContent.Length > charLimit)
                {
                    rawContent = rawContent.Substring(0, charLimit) + "... [truncated]";
                }
                sb.AppendLine($"Full source content limited to {maxTokensPerSource} tokens: {rawContent}\n");
            }
            i++;
        }
        return sb.ToString().Trim();
    }

    /// <summary>
    /// 検索結果をタイトルとURLの箇条書きリストに整形します。
    /// </summary>
    /// <param name="searchResults">'results'キーを持つ検索レスポンス</param>
    /// <returns>"* title : url"形式の文字列</returns>
    public static string FormatSources(Dictionary<string, object> searchResults)
    {
        if (!searchResults.TryGetValue("results", out var resultsObj) || resultsObj is not IEnumerable<object> results)
            return string.Empty;
        var lines = new List<string>();
        foreach (var item in results)
        {
            if (item is Dictionary<string, object> source)
            {
                string title = source.TryGetValue("title", out var t) && t is string ts ? ts : "(no title)";
                string url = source.TryGetValue("url", out var u) && u is string us ? us : "(no url)";
                lines.Add($"* {title} : {url}");
            }
        }
        return string.Join("\n", lines);
    }
}

