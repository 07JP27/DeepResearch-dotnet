# DeepResearch.SearchClient

このライブラリは、さまざまな Web 検索サービス向けの統一検索クライアントインターフェース（`ISearchClient`）とその実装を提供します。
現在は Tavily API クライアント実装が含まれています。

## 特徴

この検索クライアントライブラリは以下の機能を提供します：

- **統一インターフェース**：複数のプロバイダーで一貫した検索操作が可能な `ISearchClient`
- **Tavily 実装**：高度な検索機能を備えた Tavily API クライアント
- **拡張性の高い設計**：新しい検索サービス実装の追加が容易

## インストール

このライブラリは以下の NuGet パッケージが必要です：

```xml
<PackageReference Include="Microsoft.Extensions.Http" Version="9.0.0" />
```

## クイックスタート

推奨される利用方法は、`ISearchClient`インターフェースを通じた検索機能の利用です：

```csharp
using DeepResearch.SearchClient;
using DeepResearch.SearchClient.Tavily;

// Tavilyベースの検索クライアントを作成
var httpClient = new HttpClient();
var tavilyClient = new TavilyClient(httpClient);
var searchClient = new TavilySearchClient(tavilyClient);

// 統一インターフェースを利用
var result = await searchClient.SearchAsync(
    "人工知能のトレンド 2025年",
    maxResults: 10);

Console.WriteLine($"{result.Results.Count}件の結果が見つかりました");
foreach (var item in result.Results)
{
    Console.WriteLine($"タイトル: {item.Title}");
    Console.WriteLine($"URL: {item.Url}");
    Console.WriteLine("---");
}
```

## ISearchClient インターフェース

統一検索インターフェースは、異なる検索プロバイダー間で一貫した API を提供します：

```csharp
public interface ISearchClient
{
    Task<SearchResult> SearchAsync(string query, int maxResults, CancellationToken cancellationToken = default);
}
```

## 依存性注入

依存性注入を利用するアプリケーションでは、検索クライアントを以下のように登録できます：

```csharp
// Tavilyを検索クライアント実装として登録
services.AddHttpClient<TavilyClient>();
services.AddScoped<ITavilyClient, TavilyClient>();
services.AddScoped<ISearchClient, TavilySearchClient>();
```
