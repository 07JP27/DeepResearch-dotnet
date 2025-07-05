using DeepResearch.Core;
using Azure.AI.OpenAI;
using Azure;
using DeepResearch.SearchClient.Tavily;


var searchClient = new TavilySearchClient(
    new TavilyClient(
        new HttpClient(),
        Environment.GetEnvironmentVariable("TAVILY_API_KEY") ?? throw new Exception()
    )
);

var result = await searchClient.SearchAsync("AIの最新動向について調査", maxResults: 5);

Console.WriteLine("Search Results:");
foreach (var item in result.Results)
{
    Console.WriteLine($"Title: {item.Title}");
    Console.WriteLine($"URL: {item.Url}");
    Console.WriteLine($"Content: {item.Content[..Math.Min(200, item.Content.Length)]}...");
    Console.WriteLine("---");
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

/*
var ChatClient = new AzureOpenAIClient(
    new Uri("https://your-openai-endpoint.openai.azure.com/"),
    new AzureKeyCredential("your-api-key")
).GetChatClient("gpt-35-turbo");

var service = new DeepResearchService(ChatClient);
var result = await service.RunResearchAsync("AIの最新動向について調査", CancellationToken.None);
Console.WriteLine(result);
*/