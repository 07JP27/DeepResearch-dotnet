using DeepResearch.Core;
using DeepResearch.Core.Events;
using DeepResearch.SearchClient.Tavily;
using Azure.AI.OpenAI;
using Azure.Identity;

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

var ChatClient = new AzureOpenAIClient(
    new Uri(Environment.GetEnvironmentVariable("AOAI_BASE_URL") ?? throw new Exception("AOAI_BASE_URL is not set.")),
    new DefaultAzureCredential()
).GetChatClient("o4-mini");

// 進捗状況を表示するイベントハンドラー
void OnProgressChanged(ResearchProgress progress)
{
    var timestamp = progress.Timestamp.ToString("HH:mm:ss");

    switch (progress.Type)
    {
        case ProgressTypes.GenerateQuery:
            Console.WriteLine($"[{timestamp}] 🔍 検索クエリを生成中...");
            if (progress.Data is System.Text.Json.JsonElement json && json.TryGetProperty("query", out var query))
            {
                Console.WriteLine($"  → 検索クエリ: {query.GetString()}");
            }
            break;

        case ProgressTypes.WebResearch:
            Console.WriteLine($"[{timestamp}] 🌐 Web検索を実行中...");
            break;

        case ProgressTypes.Summarize:
            Console.WriteLine($"[{timestamp}] 📝 検索結果を要約中...");
            break;

        case ProgressTypes.Reflection:
            Console.WriteLine($"[{timestamp}] 🤔 研究結果を分析中...");
            break;

        case ProgressTypes.Routing:
            Console.WriteLine($"[{timestamp}] 🔄 次のステップを決定中...");
            break;

        case ProgressTypes.Finalize:
            Console.WriteLine($"[{timestamp}] ✅ 最終レポートを作成中...");
            break;

        case ProgressTypes.ResearchComplete:
            Console.WriteLine($"[{timestamp}] 🎉 調査が完了しました！");
            break;

        default:
            Console.WriteLine($"[{timestamp}] ⚙️  {progress.Type}を実行中...");
            break;
    }
}

var service = new DeepResearchService(ChatClient, searchClient, OnProgressChanged);
var result = await service.RunResearchAsync(researchTopic, CancellationToken.None);

Console.WriteLine("\n" + new string('=', 50));
Console.WriteLine("📋 調査結果");
Console.WriteLine(new string('=', 50));
Console.WriteLine(result.RunningSummary);

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
