using System.ComponentModel.DataAnnotations;

namespace DeepResearch.DurableFunctions.Options;
public class DeepResearchAppOptions
{
    [Required]
    public string AzureOpenAIDeploymentName { get; set; } = string.Empty;

    [Required]
    public string TavilyApiKey { get; set; } = string.Empty;
}
