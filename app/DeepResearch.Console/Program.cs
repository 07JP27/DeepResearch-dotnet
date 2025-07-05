using DeepResearch.Core;
using Azure.AI.OpenAI;
using Azure.Identity;
using DeepResearch.SearchClient.Tavily;

// dotnet runで実行する場合は以下の環境変数を設定してください
// export TAVILY_API_KEY="your-tavily-api-key-here"
// export AOAI_BASE_URL="https://your-aoai-endpoint.openai.azure.com/"

// vscodeのデバッグ構成でデバッグする場合は app/.envファイルを作成し、以下の内容を追加してください
// TAVILY_API_KEY=your-tavily-api-key-here
// AOAI_BASE_URL=https://your-aoai-endpoint.openai.azure.com/

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

var service = new DeepResearchService(ChatClient, searchClient);
var result = await service.RunResearchAsync("AIの最新動向について調査", CancellationToken.None);
Console.WriteLine(result);


Console.WriteLine("Press any key to exit...");
Console.ReadKey();