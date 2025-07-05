using DeepResearch.SearchClient;
using DeepResearch.SearchClient.Tavily;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DeepResearch.Console;

/// <summary>
/// Demo program showing unified search client usage
/// </summary>
public class TavilyDemo
{
    private readonly ISearchClient _searchClient;
    private readonly ILogger<TavilyDemo> _logger;

    public TavilyDemo(ISearchClient searchClient, ILogger<TavilyDemo> logger)
    {
        _searchClient = searchClient;
        _logger = logger;
    }

    public async Task RunDemoAsync()
    {
        _logger.LogInformation("Starting Tavily Client Demo...");

        try
        {
            await DemoBasicSearch();

            _logger.LogInformation("Demo completed successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Demo failed: {Message}", ex.Message);
        }
    }

    private async Task DemoBasicSearch()
    {
        _logger.LogInformation("\n=== Demo 1: Basic Search ===");
        
        var result = await _searchClient.SearchAsync(
            "latest developments in artificial intelligence 2024",
            maxResults: 3);

        _logger.LogInformation("Found {Count} results", result.Results.Count);
        
        foreach (var item in result.Results.Take(2))
        {
            _logger.LogInformation("Title: {Title}", item.Title);
            _logger.LogInformation("URL: {Url}", item.Url);
            _logger.LogInformation("Content: {Content}", item.Content[..Math.Min(200, item.Content.Length)]+"...");
            _logger.LogInformation("---");
        }
    }

    public static async Task RunAsync(string[] args)
    {
        // Create host with dependency injection
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Register HttpClient
                services.AddHttpClient<TavilyClient>();
                
                // Register Tavily client
                services.AddScoped<ITavilyClient, TavilyClient>();
                
                // Register unified search client
                services.AddScoped<ISearchClient, TavilySearchClient>();
                
                // Register demo
                services.AddScoped<TavilyDemo>();
            })
            .Build();

        // Run demo
        var demo = host.Services.GetRequiredService<TavilyDemo>();
        await demo.RunDemoAsync();
    }
}
