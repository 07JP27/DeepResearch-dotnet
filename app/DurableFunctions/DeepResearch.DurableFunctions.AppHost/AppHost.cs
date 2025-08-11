var builder = DistributedApplication.CreateBuilder(args);

var aoai = builder.AddConnectionString("aoai");
var deploymentName = builder.AddParameter("AzureOpenAIDeploymentName");
var tavily = builder.AddParameter("TavilyApiKey");

builder.AddAzureFunctionsProject<Projects.DeepResearch_DurableFunctions>("deepresearch-durablefunctions")
    .WithReference(aoai)
    .WithEnvironment("DeepResearchAppOptions__AzureOpenAIDeploymentName", deploymentName)
    .WithEnvironment("DeepResearchAppOptions__TavilyApiKey", tavily);

builder.Build().Run();
