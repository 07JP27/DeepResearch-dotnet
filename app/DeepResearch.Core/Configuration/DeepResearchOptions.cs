namespace DeepResearch.Core.Configuration;

public class DeepResearchOptions
{
    public string AzureEndpoint { get; set; }
    public string AzureApiKey { get; set; }
    public string ModelName { get; set; }
    public string SearchApiKey { get; set; }
    public int MaxResearchLoops { get; set; } = 3;
    public int MaxTokensPerSource { get; set; } = 1000;
}
