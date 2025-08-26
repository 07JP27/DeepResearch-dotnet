using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var aoai = builder.AddConnectionString("aoai");
var signalr = builder.AddConnectionString("signalr", "AzureSignalRConnectionString");
var deploymentName = builder.AddParameter("AzureOpenAIDeploymentName");
var tavily = builder.AddParameter("TavilyApiKey");

var deepResearchDurableFunctions = builder.AddAzureFunctionsProject<Projects.DeepResearch_DurableFunctions>("deepresearch-durablefunctions")
    .WithReference(aoai)
    .WithEnvironment("AzureSignalRConnectionString", signalr)
    .WithEnvironment("DeepResearchAppOptions__AzureOpenAIDeploymentName", deploymentName)
    .WithEnvironment("DeepResearchAppOptions__TavilyApiKey", tavily);

builder.AddProject<Projects.DeepResearch_DurableFunctions_Web>("deepresearch-durablefunctions-web")
    .WithReference(deepResearchDurableFunctions);

builder.Build().Run();
