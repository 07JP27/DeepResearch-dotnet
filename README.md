[日本語はこちら](README_ja.md)

# DeepResearch .NET

This is a .NET class library version of [Azure-Samples/deepresearch](https://github.com/Azure-Samples/deepresearch).
An AI-powered automated research system that progressively gathers information on specified topics and generates comprehensive reports.

## Overview

DeepResearch .NET is an AI-driven research tool with the following features:

- **Iterative Research**: AI autonomously generates search queries and repeats information gathering
- **Knowledge Gap Analysis**: Analyzes collected information and identifies missing parts
- **Comprehensive Report Generation**: Integrates collected information and creates structured reports
- **Real-time Progress Display**: Track each stage of research in real-time

## Directory Structure

```
DeepResearch-dotnet/
└── app/
    ├── DeepResearch.Core -----------> Core library for DeepResearch
    ├── DeepResearch.SearchClient ---> Search client for retrieving information for DeepResearch
    ├── DeepResearch.Console---------> Sample console application
    └── DeepResearch.Web-------------> Sample web application UI
```

## Prerequisites

### Required Services

1. **Azure OpenAI Service**

   - Assumes o4-mini model
   - Please note down your endpoint URL and deployment name

2. **Tavily Search API**

   - Get an API key from [Tavily](https://tavily.com/)

3. Configure various information according to the client you use:
   - **DeepResearch.Console**: See comments in Program.cs
   - **DeepResearch.Web**: For local debugging, add to appsettings.Development.json
   ```json
   {
     "OpenAI": {
       "Endpoint": "YOUR_OPENAI",
       "DeploymentName": "YOUR_DEPLOYMENT_NAME"
     },
     "Tavily": {
       "ApiKey": "YOUR_TAVILY_API_KEY"
     }
   }
   ```

## Usage

```csharp

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

var options = new DeepResearchOptions
{
    MaxResearchLoops = 3, // Maximum number of loops
    MaxTokensPerSource = 1000, // Maximum tokens per source
    MaxSourceCountPerSearch = 5 // Maximum number of sources per search
};

Console.WriteLine("Deep Research Console");
Console.WriteLine("====================");
Console.Write("Enter the topic you want to research: ");
var researchTopic = Console.ReadLine();

var service = new DeepResearchService(ChatClient, searchClient, OnProgressChanged, options);
var result = await service.RunResearchAsync(researchTopic, CancellationToken.None);

Console.WriteLine("\n" + new string('=', 50));
Console.WriteLine("📋 Research Results");
Console.WriteLine(new string('=', 50));
Console.WriteLine(result.RunningSummary);

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
```

## Sample Clients

### Console App

The [DeepResearch.Console](/app/DeepResearch.Console/) project contains a sample console application.
![DeepResearch.Console](/images/consoleapp.png)

### Web App

The [DeepResearch.Web](/app/DeepResearch.Web/) project contains a sample web application using Blazor.
![DeepResearch.Web](/images/webapp1.png)
![DeepResearch.Web](/images/webapp2.png)

Demo video: https://youtu.be/J49-Pywa2EM?si=Gdv5kisPSaMUq3W_

## Extensions

By implementing your own search client that implements the ISearchClient interface defined in DeepResearch.SearchClient, you can easily add searches for other data sources such as internal company information or specific databases, not just web searches.

```mermaid
flowchart TD
    %% Core Classes
    DeepResearchService[DeepResearchService<br/>Core Research Engine]


    %% Search Client Interface and Implementations
    ISearchClient[ISearchClient<br/>Search Interface]
    TavilySearchClient[TavilySearchClient<br/>Tavily Search Implementation]
    CustomSearchClient[CustomSearchClient<br/>Custom Search Implementation]

    %% Sample Applications
    ConsoleApp[DeepResearch.Console<br/>Console App]
    WebApp[DeepResearch.Web<br/>Web App]

    %% Dependencies
    DeepResearchService -->|uses| ISearchClient

    TavilySearchClient -.->|implements| ISearchClient
    CustomSearchClient -.->|implements| ISearchClient

    ConsoleApp -->|uses| DeepResearchService
    WebApp -->|uses| DeepResearchService

    %% Styling
    classDef interface fill:#bbdefb,stroke:#0d47a1,stroke-width:2px,color:#000000
    classDef implementation fill:#e1bee7,stroke:#4a148c,stroke-width:2px,color:#000000
    classDef core fill:#c8e6c9,stroke:#1b5e20,stroke-width:2px,color:#000000
    classDef app fill:#f8bbd9,stroke:#880e4f,stroke-width:2px,color:#000000

    class ISearchClient interface
    class TavilySearchClient,CustomSearchClient implementation
    class DeepResearchService core
    class ConsoleApp,WebApp app
```
