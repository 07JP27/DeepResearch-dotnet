using DeepResearch.Core.SearchClient;

namespace DeepResearch.Core;

/// <summary>
/// Represents the final result of a research operation, containing only the information needed by clients.
/// </summary>
public class ResearchResult
{
    /// <summary>
    /// The topic that was researched.
    /// </summary>
    public string ResearchTopic { get; set; } = string.Empty;

    /// <summary>
    /// The final summary of the research findings.
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// The structured source information used in the research.
    /// </summary>
    public List<SearchResultItem> Sources { get; set; } = new();

    /// <summary>
    /// Images found during the research process.
    /// </summary>
    public List<string> Images { get; set; } = new();
}
