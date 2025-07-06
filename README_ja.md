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

var ChatClient = new AzureOpenAIClient(
    new Uri(Environment.GetEnvironmentVariable("AOAI_BASE_URL") ?? throw new Exception("AOAI_BASE_URL is not set.")),
    new DefaultAzureCredential()
).GetChatClient("o4-mini");

var options = new DeepResearchOptions
{
    MaxResearchLoops = 3, // 最大ループ数
    MaxTokensPerSource = 1000, // ソースごとの最大トークン数
    MaxSourceCountPerSearch = 5 // 検索ごとの最大ソース数
};

Console.WriteLine("Deep Research Console");
Console.WriteLine("====================");
Console.Write("調査したいトピックを入力してください: ");
var researchTopic = Console.ReadLine();

var service = new DeepResearchService(ChatClient, searchClient, OnProgressChanged, options);
var result = await service.RunResearchAsync(researchTopic, CancellationToken.None);

Console.WriteLine("\n" + new string('=', 50));
Console.WriteLine("📋 調査結果");
Console.WriteLine(new string('=', 50));
Console.WriteLine(result.RunningSummary);

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
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
