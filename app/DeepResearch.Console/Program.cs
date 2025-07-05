using DeepResearch.Core;
using Azure.AI.OpenAI;
using Azure;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var ChatClient = new AzureOpenAIClient(
    new Uri("https://your-openai-endpoint.openai.azure.com/"),
    new AzureKeyCredential("your-api-key")
).GetChatClient("gpt-35-turbo");

//var service = new DeepResearchService(ChatClient);
//var result = await service.RunResearchAsync("AIの最新動向について調査", CancellationToken.None);