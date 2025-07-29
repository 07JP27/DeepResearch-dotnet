using Azure.AI.OpenAI;
using Azure.Identity;
using DeepResearch.Core.SearchClient;
using DeepResearch.SearchClient.Tavily;
using LongRunningDeepResearch.ChatClient;
using LongRunningDeepResearch.SearchClient;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets<Program>();

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Add HttpClient
builder.Services.AddHttpClient();

builder.Services.AddKeyedSingleton(GetResponseActivity.ChatClientKey, (provider, _) =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    // Configure OpenAI
    var openAIEndpoint = configuration["OpenAI:Endpoint"];
    var deploymentName = configuration["OpenAI:DeploymentName"];

    if (string.IsNullOrEmpty(openAIEndpoint) || string.IsNullOrEmpty(deploymentName))
    {
        throw new InvalidOperationException("OpenAI endpoint or deployment name must be configured.");
    }

    var openAIClient = new AzureOpenAIClient(new(openAIEndpoint), new DefaultAzureCredential());
    var chatClient = openAIClient.GetChatClient(deploymentName);
    return chatClient.AsIChatClient();
});


builder.Services.AddScoped<ITavilyClient>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    // Configure Tavily Search Client
    var tavilyApiKey = configuration["Tavily:ApiKey"];
    if (string.IsNullOrEmpty(tavilyApiKey))
    {
        throw new InvalidOperationException("Tavily API key must be configured.");
    }

    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();
    return new TavilyClient(httpClient, tavilyApiKey);
});
builder.Services.AddKeyedScoped<ISearchClient, TavilySearchClient>(SearchActivity.SearchClientKey);

builder.Build().Run();
