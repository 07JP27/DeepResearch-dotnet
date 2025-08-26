using Azure.AI.OpenAI;
using DeepResearch.DurableFunctions.Adapters.Activities;
using DeepResearch.DurableFunctions.Options;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.AI;
using DeepResearch.SearchClient.Tavily;
using DeepResearch.Core.SearchClient;
using DeepResearch.DurableFunctions.SignalR;

var builder = FunctionsApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets<Program>();
builder.AddServiceDefaults();

builder.ConfigureFunctionsWebApplication();

// Register ServerlessHub for SignalR
builder.Services.AddServerlessHub<NotifyProgressActivity>();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();
builder.Services.AddOptions<DeepResearchAppOptions>()
    .BindConfiguration(nameof(DeepResearchAppOptions))
    .ValidateDataAnnotations();
builder.AddAzureOpenAIClient("aoai");
builder.Services.AddKeyedChatClient(
    DurableChatClientActivity.ChatClientKey, 
    sp =>
    {
        var options = sp.GetRequiredService<IOptions<DeepResearchAppOptions>>().Value;
        var aoaiClient = sp.GetRequiredService<AzureOpenAIClient>();
        return aoaiClient.GetChatClient(options.AzureOpenAIDeploymentName)
            .AsIChatClient();
    }).UseOpenTelemetry();
builder.Services.AddScoped<ITavilyClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<DeepResearchAppOptions>>().Value;
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    return new TavilyClient(httpClientFactory.CreateClient(), options.TavilyApiKey);
});
builder.Services.AddKeyedScoped<ISearchClient>(
    DurableSearchClientActivity.SearchClientKey,
    (sp, _) =>
    {
        var options = sp.GetRequiredService<IOptions<DeepResearchAppOptions>>().Value;
        return new TavilySearchClient(sp.GetRequiredService<ITavilyClient>());
    });

builder.Build().Run();
