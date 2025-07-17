using System.Text;
using System.Text.RegularExpressions;
using DeepResearch.Core.SearchClient;

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
        if (searchResult == null || searchResult.Results == null || searchResult.Results is [])
            return string.Empty;

        // URLで重複排除
        var uniqueSources = searchResult.Results.DistinctBy(x => x.Url);

        var sb = new StringBuilder();
        sb.AppendLine("Sources:\n");
        foreach (var source in uniqueSources)
        {
            string title = !string.IsNullOrEmpty(source.Title) ? CleanGarbledText(source.Title) : "(no title)";
            string url = !string.IsNullOrEmpty(source.Url) ? source.Url : "(no url)";
            string content = !string.IsNullOrEmpty(source.Content) ? CleanGarbledText(source.Content) : "";
            sb.AppendLine($"Source: {title}\n===");
            sb.AppendLine($"URL: {url}\n===");
            sb.AppendLine($"Most relevant content from source: {content}\n===");
            if (fetchFullPage)
            {
                string rawContent = !string.IsNullOrEmpty(source.RawContent) ? CleanGarbledText(source.RawContent) : "";
                if (rawContent.Length > maxCharacterPerSource)
                {
                    rawContent = rawContent.Substring(0, maxCharacterPerSource) + "... [truncated]";
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
    /// 新しいソースを既存のソースと重複排除し、テキストをクリーニングします。
    /// </summary>
    /// <param name="newSources">新しく追加するソースリスト</param>
    /// <param name="existingSources">既存のソースリスト</param>
    /// <returns>重複排除・クリーニング済みの新しいソースリスト</returns>
    internal static List<SearchResultItem> DeduplicateAndCleanSources(
        List<SearchResultItem> newSources,
        List<SearchResultItem> existingSources)
    {
        // Create a set of existing URLs for quick lookup
        var existingUrls = new HashSet<string>(existingSources.Select(s => s.Url));

        var deduplicatedSources = new List<SearchResultItem>();

        foreach (var source in newSources)
        {
            // Skip if URL already exists
            if (string.IsNullOrEmpty(source.Url) || existingUrls.Contains(source.Url))
                continue;

            // Clean the source data
            var cleanedSource = new SearchResultItem
            {
                Title = CleanGarbledText(source.Title),
                Url = source.Url,
                Content = CleanGarbledText(source.Content),
                RawContent = CleanGarbledText(source.RawContent)
            };

            deduplicatedSources.Add(cleanedSource);
            existingUrls.Add(source.Url); // Add to set to prevent duplicates within this batch
        }

        return deduplicatedSources;
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
            // 特定の文字化けパターンを先に処理（より具体的なパターンから）
            .Replace("ã§", "で")
            .Replace("ã¯", "は")
            .Replace("ã»", "・")
            .Replace("ã¾", "ま")
            .Replace("ã¤", "い")
            .Replace("ã ", "")
            // 連続する文字化け文字
            .Replace("ãã", "")
            // 単独の文字化け文字（具体的なパターンの後に処理）
            .Replace("ã", "")
            .Replace("â", "")
            .Replace("¿", "")
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

