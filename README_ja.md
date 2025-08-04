[English is here ](README.md)

# DeepResearch .NET

[Azure-Samples/deepresearch](https://github.com/Azure-Samples/deepresearch)の.NET 版クラスライブラリです。
AI を活用した自動リサーチシステムで、指定されたトピックについて段階的に情報収集し、包括的なレポートを生成します。

## 概要

DeepResearch .NET は、以下の特徴を持つ AI 駆動の調査ツールです：

- **反復的リサーチ**: AI が自律的に検索クエリを生成し、情報収集を繰り返します
- **知識ギャップ分析**: 収集した情報を分析し、不足している部分を特定します
- **包括的レポート生成**: 収集した情報を統合し、構造化されたレポートを作成します
- **リアルタイム進行状況表示**: 調査の各段階をリアルタイムで追跡できます

## ディレクトリ構成

```
DeepResearch-dotnet/
└── app/
    ├── DeepResearch.Core -----------> DeepResearchのコアライブラリ
    ├── DeepResearch.SearchClient ---> DeepResearchを行うために情報を取得するための検索クライアント
    ├── DeepResearch.Console---------> サンプルのコンソールアプリケーション
    └── DeepResearch.Web-------------> サンプルのWebアプリケーションのUI
```

## 事前準備

### 必要なサービス

1. **Azure OpenAI Service**

   - o4-mini モデルを想定しています
   - エンドポイント URL とデプロイメント名を控えておいてください

2. **Tavily Search API**

   - [Tavily](https://tavily.com/)で API キーを取得してください

3. 使用するクライアントに応じて各種情報を設定します。
   - **DeepResearch.Console** : Program.cs のコメント参照。
   - **DeepResearch.Web** : ローカルデバッグ時は appsettings.Development.json に記載
   ```json
   {
     "OpenAI": {
       "Endpoint": "YOUR_OPENAI",
       "DeploymentName": "YOUR_DEPLOYMENT_NAME"
     },
     "Tavily": {
       "ApiKey": "YOUR_TAVILY_API_KEY"
     }
   }
   ```

## 使い方

```csharp

var searchClient = new TavilySearchClient(
    new TavilyClient(
        new HttpClient(),
        Environment.GetEnvironmentVariable("TAVILY_API_KEY") ?? throw new Exception("TAVILY_API_KEY is not set.")
    )
);

var chatClient = new AzureOpenAIClient(
    new Uri(Environment.GetEnvironmentVariable("AOAI_BASE_URL") ?? throw new Exception("AOAI_BASE_URL is not set.")),
    new DefaultAzureCredential()
).GetChatClient("o4-mini");

void OnProgressChanged(ProgressBase progress)
{
  // Handle progress updates here
}

var options = new DeepResearchOptions
{
    MaxResearchLoops = 3, // 最大ループ数
    MaxCharacterPerSource = 1000, // ソースごとの最大文字数
    MaxSourceCountPerSearch = 5, // 検索ごとの最大ソース数
    EnableSummaryConsolidation = true // 最終回答の生成にすべてのステップサマリーを統合するかどうかのフラグ。Trueにする場合はコンテキスト長の制限に注意して上の３つのパラメーターを調整する。
};

Console.WriteLine("Deep Research Console");
Console.WriteLine("====================");
Console.Write("調査したいトピックを入力してください: ");
var researchTopic = Console.ReadLine();

// TimeProviderを使用してDeepResearchServiceを作成
var timeProvider = TimeProvider.System;
var service = new DeepResearchService(chatClient, searchClient, timeProvider);

// プログレスコールバックを作成
var progress = new Progress<ProgressBase>(OnProgressChanged);

var result = await service.RunResearchAsync(researchTopic, options, progress, CancellationToken.None);

Console.WriteLine("\n" + new string('=', 50));
Console.WriteLine("📋 調査結果");
Console.WriteLine(new string('=', 50));
Console.WriteLine(result.Summary);

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
```

## 戻り値と逐次通知

DeepResearchService は、調査が完了した際に ResearchResult 型のオブジェクトを返します。
このオブジェクトには、調査の要約や関連する情報が含まれています。

- ResearchTopic : 調査対象のトピック
- Summary : 調査の最終レポート
- Sources : 調査中に収集された情報のリスト
- Images : 調査中に収集された画像のリスト

また、`RunResearchAsync` メソッドに `IProgress<ProgressBase>` パラメータを渡すことで、調査の進捗状況をリアルタイムで受け取ることができます。
逐次通知は ProgressBase を継承した各ステップごとのクラスが定義されています。

- QueryGenerationProgress: クエリ生成完了の通知クラス
- WebResearchProgress: Web 検索完了の通知クラス
- SummarizeProgress: 検索結果の要約完了の通知クラス
- ReflectionProgress: 知識ギャップの補完完了の通知クラス
- RoutingProgress: 次の処理の判定完了の通知クラス
- FinalizeProgress: 調査レポートの作成開始の通知クラス
- ResearchCompleteProgress: 調査レポートの作成完了の通知クラス
- ErrorProgress: 調査中にエラーが発生した際の通知クラス

例えば以下のように通知クラスを受けとってハンドルすることができます。

```csharp
void OnProgressChanged(ProgressBase progress)
{
    switch (progress)
    {
        case QueryGenerationProgress queryProgress:
            Console.WriteLine($"クエリを生成: {queryProgress.Query}");
            Console.WriteLine($"クエリ生成の理由: {queryProgress.Rationale}");
            break;
        case WebResearchProgress webProgress:
            Console.WriteLine($"Web検索完了： {webProgress.Sources.Count} 件のソースを取得");
            Console.WriteLine($"Web検索完了： {webProgress.Images.Count} 枚の画像を取得");
            break;
        case SummarizeProgress summarizeProgress:
            Console.WriteLine($"要約: {summarizeProgress.Summary}");
            break;
        case ReflectionProgress reflectionProgress:
            Console.WriteLine("リフレクション完了");
            Console.WriteLine($"知識ギャップ: {reflectionProgress.KnowledgeGap}");
            Console.WriteLine($"追加検索クエリ: {reflectionProgress.Query}");
            break;
        case RoutingProgress routingProgress:
            Console.WriteLine($"ルートの決定 {routingProgress.Decision}");
            Console.WriteLine($"ループ思考回数 {routingProgress.LoopCount}");
            break;
        case FinalizeProgress finalizeProgress:
            Console.WriteLine("最終レポートの作成中.");
            break;
        case ResearchCompleteProgress completeProgress:
            Console.WriteLine("調査の完了");
            Console.WriteLine($"Final Summary: {completeProgress.FinalSummary}");
            Console.WriteLine($"参考情報: {string.Join(", ", completeProgress.Sources)}");
            Console.WriteLine($"画像: {string.Join(", ", completeProgress.Images)}");
            break;
        case ErrorProgress errorProgress:
            Console.WriteLine($"エラーが発生しました。: {errorProgress.Message}");
            break;
        default:
            Console.WriteLine("不明な通知型");
            break;
    }
}
```

## 非同期プログレスコールバック

DeepResearch .NET は `IAsyncProgress<T>` インターフェースを通じて非同期プログレスコールバックをサポートしており、プログレス処理自体が非同期である必要がある場合（データベースログ、ネットワーク呼び出し、複雑なUI更新など）に適しています。

### IAsyncProgress<T> の使用方法

非同期プログレスコールバックは、いくつかの方法で使用できます：

#### 1. 簡単な非同期操作に AsyncProgress.FromFunc を使用:

```csharp
var asyncProgress = AsyncProgress.FromFunc<ProgressBase>(async progress =>
{
    // 例：データベースに非同期でログを記録
    await LogProgressToDatabase(progress);
    
    // 例：UIを非同期で更新
    await UpdateUI(progress);
});

var result = await service.RunResearchAsync(researchTopic, options, asyncProgress, CancellationToken.None);
```

#### 2. キャンセレーションサポートが必要な操作に AsyncProgress.Create を使用:

```csharp
var asyncProgress = AsyncProgress.Create<ProgressBase>(async (progress, cancellationToken) =>
{
    // キャンセレーションサポート付きでプログレスを処理
    await ProcessProgressAsync(progress, cancellationToken);
});

var result = await service.RunResearchAsync(researchTopic, asyncProgress, CancellationToken.None);
```

#### 3. 既存の IProgress<T> を IAsyncProgress<T> に変換:

```csharp
var progress = new Progress<ProgressBase>(OnProgressChanged);
var asyncProgress = progress.ToAsyncProgress(); // 拡張メソッド

var result = await service.RunResearchAsync(researchTopic, options, asyncProgress, CancellationToken.None);
```

### 利用可能なメソッドオーバーロード

DeepResearchService は、さまざまな使用パターンに対応するため、複数のオーバーロードを提供しています：

```csharp
// 基本的な使用方法
await service.RunResearchAsync(topic);

// オプション付き
await service.RunResearchAsync(topic, options);

// 同期プログレス付き
await service.RunResearchAsync(topic, options, progress, cancellationToken);

// 非同期プログレス付き
await service.RunResearchAsync(topic, options, asyncProgress, cancellationToken);

// 非同期プログレスのみ
await service.RunResearchAsync(topic, asyncProgress, cancellationToken);
```

## サンプルクライアント

### コンソールアプリ

[DeepResearch.Console](/app/DeepResearch.Console/) プロジェクトには、コンソールアプリケーションのサンプルが含まれています。
![DeepResearch.Console](/images/consoleapp.png)

### Web アプリ

[DeepResearch.Web](/app/DeepResearch.Web/) プロジェクトには、Blazor を使用した Web アプリケーションのサンプルが含まれています
![DeepResearch.Web](/images/webapp1.png)
![DeepResearch.Web](/images/webapp2.png)

動作デモ：https://youtu.be/J49-Pywa2EM?si=Gdv5kisPSaMUq3W_

## 拡張

DeepResearch.SearchClient で定義された ISearchClient インターフェースを実装した独自の検索クライアントを実装することで、Web 検索だけでなく社内情報や特定のデータベスなど、他のデータソース検索を簡単に追加できます。

```mermaid
flowchart TD
    %% Core Classes
    DeepResearchService[DeepResearchService<br/>Core調査エンジン]


    %% Search Client Interface and Implementations
    ISearchClient[ISearchClient<br/>検索インターフェース]
    TavilySearchClient[TavilySearchClient<br/>Tavily検索実装]
    CustomSearchClient[CustomSearchClient<br/>カスタム検索実装]

    %% Sample Applications
    ConsoleApp[DeepResearch.Console<br/>コンソールアプリ]
    WebApp[DeepResearch.Web<br/>Webアプリ]

    %% Dependencies
    DeepResearchService -->|uses| ISearchClient

    TavilySearchClient -.->|implements| ISearchClient
    CustomSearchClient -.->|implements| ISearchClient

    ConsoleApp -->|uses| DeepResearchService
    WebApp -->|uses| DeepResearchService

    %% Styling
    classDef interface fill:#bbdefb,stroke:#0d47a1,stroke-width:2px,color:#000000
    classDef implementation fill:#e1bee7,stroke:#4a148c,stroke-width:2px,color:#000000
    classDef core fill:#c8e6c9,stroke:#1b5e20,stroke-width:2px,color:#000000
    classDef app fill:#f8bbd9,stroke:#880e4f,stroke-width:2px,color:#000000

    class ISearchClient interface
    class TavilySearchClient,CustomSearchClient implementation
    class DeepResearchService core
    class ConsoleApp,WebApp app
```
