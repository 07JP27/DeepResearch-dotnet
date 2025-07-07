using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DeepResearch.SearchClient;

namespace DeepResearch.Core;

internal static class Formatting
{
    /// <summary>
    /// 検索APIのレスポンスを重複排除し、構造化された文字列に整形します。
    /// </summary>
    /// <param name="searchResponse">'results'キーを持つ辞書、またはそのリスト</param>
    /// <param name="maxCharacterPerSource">各ソースの最大文字数</param>
    /// <param name="fetchFullPage">全文を含めるかどうか</param>
    /// <returns>重複排除・整形済み文字列</returns>
    // SearchResult型を受け取るオーバーロード
    internal static string DeduplicateAndFormatSources(
        SearchResult searchResult,
        int maxCharacterPerSource,
        bool fetchFullPage = true)
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
            string title = !string.IsNullOrEmpty(source.Title) ? CleanGarbledText(source.Title) : "(no title)";
            string url = !string.IsNullOrEmpty(source.Url) ? source.Url : "(no url)";
            string content = !string.IsNullOrEmpty(source.Content) ? CleanGarbledText(source.Content) : "";
            sb.AppendLine($"Source: {title}\n===");
            sb.AppendLine($"URL: {url}\n===");
            sb.AppendLine($"Most relevant content from source: {content}\n===");
            if (fetchFullPage)
            {
                int charLimit = maxCharacterPerSource;
                string rawContent = !string.IsNullOrEmpty(source.RawContent) ? CleanGarbledText(source.RawContent) : "";
                if (rawContent.Length > charLimit)
                {
                    rawContent = rawContent.Substring(0, charLimit) + "... [truncated]";
                }
                sb.AppendLine($"Full source content limited to {maxCharacterPerSource} tokens: {rawContent}\n");
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
    internal static string FormatSources(SearchResult searchResult)
    {
        if (searchResult == null || searchResult.Results == null)
            return string.Empty;
        var lines = new List<string>();
        foreach (var item in searchResult.Results)
        {
            string title = !string.IsNullOrEmpty(item.Title) ? CleanGarbledText(item.Title) : "(no title)";
            string url = !string.IsNullOrEmpty(item.Url) ? item.Url : "(no url)";
            lines.Add($"* {title} : {url}");
        }
        return string.Join("\n", lines);
    }

    /// <summary>
    /// 文字化けしたテキストをクレンジングします。
    /// </summary>
    /// <param name="text">クレンジング対象のテキスト</param>
    /// <returns>クレンジング済みのテキスト</returns>
    private static string CleanGarbledText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // 日本語が含まれているかチェック
        if (Regex.IsMatch(text, @"[\u3040-\u309F\u30A0-\u30FF\u4E00-\u9FAF]"))
        {
            // 日本語が含まれている場合は、文字化けパターンのみクリーンアップ
            return CleanSpecificGarbledPatterns(text);
        }

        return text;
    }

    /// <summary>
    /// 特定の文字化けパターンをクリーンアップします。
    /// </summary>
    /// <param name="text">クリーンアップ対象のテキスト</param>
    /// <returns>クリーンアップ済みのテキスト</returns>
    private static string CleanSpecificGarbledPatterns(string text)
    {
        // 特定の文字化けパターンのみを対象とした置換
        var cleanedText = text
            // 明らかな文字化けパターンのみを削除（保守的なアプローチ）
            .Replace("ã", "")
            .Replace("â", "")
            .Replace("¿", "")
            // 連続する文字化け文字
            .Replace("ãã", "")
            .Replace("ã§", "で")
            .Replace("ã¯", "は")
            .Replace("ã ", "")
            .Replace("ã»", "・")
            .Replace("ã¾", "ま")
            .Replace("ã", "")
            .Replace("ã¤", "い")
            // 制御文字を削除
            .Replace("\0", "")
            .Replace("\r", "")
            .Trim();

        // 不正な制御文字と明らかに不要な記号のみを削除
        cleanedText = Regex.Replace(cleanedText, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", "");

        // 連続する空白を単一の空白に
        cleanedText = Regex.Replace(cleanedText, @"\s+", " ");

        return cleanedText.Trim();
    }
}

