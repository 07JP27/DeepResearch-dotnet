using DeepResearch.Web.Components;
using DeepResearch.Web.Services;
using DeepResearch.SearchClient.Tavily;
using Azure.AI.OpenAI;
using Azure.Identity;
using OpenAI.Chat;
using DeepResearch.Core.SearchClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Controllers for API endpoints
builder.Services.AddControllers();

// Add HttpClient
builder.Services.AddHttpClient();

// Configure OpenAI
var openAIEndpoint = builder.Configuration["OpenAI:Endpoint"];
var deploymentName = builder.Configuration["OpenAI:DeploymentName"];

if (string.IsNullOrEmpty(openAIEndpoint) || string.IsNullOrEmpty(deploymentName))
{
    throw new InvalidOperationException("OpenAI endpoint or deployment name must be configured.");
}

var openAIClient = new AzureOpenAIClient(new Uri(openAIEndpoint), new DefaultAzureCredential());
var chatClient = openAIClient.GetChatClient(deploymentName);
builder.Services.AddSingleton(chatClient);

// Configure Tavily Search Client
var tavilyApiKey = builder.Configuration["Tavily:ApiKey"];
if (string.IsNullOrEmpty(tavilyApiKey))
{
    throw new InvalidOperationException("Tavily API key must be configured.");
}

builder.Services.AddScoped<ITavilyClient>(provider =>
{
    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();
    return new TavilyClient(httpClient, tavilyApiKey);
});
builder.Services.AddScoped<ISearchClient>(provider =>
{
    var tavilyClient = provider.GetRequiredService<ITavilyClient>();
    return new DeepResearch.SearchClient.Tavily.TavilySearchClient(tavilyClient);
});

// Add research services
builder.Services.AddScoped<WebResearchService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
