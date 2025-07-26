using DeepResearch.Core;
using DeepResearch.Core.Models;
using DeepResearch.SearchClient.Tavily;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;

// dotnet runで実行する場合は以下の環境変数を設定してください
// export TAVILY_API_KEY="your-tavily-api-key-here"
// export AOAI_BASE_URL="https://your-aoai-endpoint.openai.azure.com/"

// vscodeのデバッグ構成でデバッグする場合は app/.envファイルを作成し、以下の内容を追加してください
// TAVILY_API_KEY=your-tavily-api-key-here
// AOAI_BASE_URL=https://your-aoai-endpoint.openai.azure.com/

Console.WriteLine("Deep Research Console");
Console.WriteLine("====================");
Console.Write("調査したいトピックを入力してください: ");
var researchTopic = Console.ReadLine();

if (string.IsNullOrWhiteSpace(researchTopic))
{
    Console.WriteLine("トピックが入力されませんでした。プログラムを終了します。");
    return;
}

Console.WriteLine($"\n「{researchTopic}」について調査を開始します...\n");

var searchClient = new TavilySearchClient(
    new TavilyClient(
        new HttpClient(),
        Environment.GetEnvironmentVariable("TAVILY_API_KEY") ?? throw new Exception("TAVILY_API_KEY is not set.")
    )
);

var chatClient = new AzureOpenAIClient(
    new Uri(Environment.GetEnvironmentVariable("AOAI_BASE_URL") ?? throw new Exception("AOAI_BASE_URL is not set.")),
    new DefaultAzureCredential()
).GetChatClient("o4-mini")
.AsIChatClient();

// TimeProviderを明示的に作成して渡す
var timeProvider = TimeProvider.System;
var service = new DeepResearchService(chatClient, searchClient, timeProvider);

// 進捗状況を表示するイベントハンドラー
void OnProgressChanged(ProgressBase progress)
{
    var timestamp = progress.Timestamp.ToString("HH:mm:ss");

    switch (progress)
    {
        case QueryGenerationProgress queryProgress:
            Console.WriteLine($"[{timestamp}] 🔍 検索クエリを生成中...");
            Console.WriteLine($"  → 検索クエリ: {queryProgress.Query}");
            break;

        case WebResearchProgress webProgress:
            Console.WriteLine($"[{timestamp}] 🌐 Web検索を実行中...");
            break;

        case SummarizeProgress summarizeProgress:
            Console.WriteLine($"[{timestamp}] 📝 検索結果を要約中...");
            break;

        case ReflectionProgress reflectionProgress:
            Console.WriteLine($"[{timestamp}] 🤔 調査結果を分析中...");
            break;

        case RoutingProgress routingProgress:
            var decisionText = routingProgress.Decision switch
            {
                RoutingDecision.Continue => "調査を続行",
                RoutingDecision.Finalize => "調査を完了",
                RoutingDecision.RetrySearch => "再検索",
                _ => routingProgress.Decision.ToString()
            };

            var content = routingProgress.Decision switch
            {
                RoutingDecision.RetrySearch => $"<strong>次の処理を判断:</strong> {decisionText} (リトライ回数：{routingProgress.LoopCount + 1})",
                _ => $"<strong>次の処理を判断:</strong> {decisionText} (ループ {routingProgress.LoopCount + 1})"
            };

            Console.WriteLine($"[{timestamp}] 🔄 次のステップを決定中: {content}");

            break;

        case FinalizeProgress finalizeProgress:
            Console.WriteLine($"[{timestamp}] ✅ 最終レポートを作成中...");
            break;

        case ResearchCompleteProgress completeProgress:
            Console.WriteLine($"[{timestamp}] 🎉 調査が完了しました！");
            break;

        default:
            Console.WriteLine($"[{timestamp}] ⚙️  {progress.Type}を実行中...");
            break;
    }
}

var options = new DeepResearchOptions
{
    MaxResearchLoops = 3, // 最大ループ数
    MaxCharacterPerSource = 1000, // ソースごとの最大文字数
    MaxSourceCountPerSearch = 5 // 検索ごとの最大ソース数
};

// 進捗状況を追跡するプログレスオブジェクトを作成
var progress = new Progress<ProgressBase>(OnProgressChanged);

var result = await service.RunResearchAsync(researchTopic, options, progress, CancellationToken.None);

Console.WriteLine("\n" + new string('=', 50));
Console.WriteLine("📋 調査結果");
Console.WriteLine(new string('=', 50));
Console.WriteLine(result.Summary);

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
