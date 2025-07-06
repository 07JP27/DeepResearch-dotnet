namespace DeepResearch.Core;

public class DeepResearchOptions
{
    /// <summary>
    /// Maximum number of research loops to perform.
    /// </summary>
    /// <example>3</example>
    public int MaxResearchLoops { get; set; } = 3;

    /// <summary>
    /// Maximum number of tokens to process per source.
    /// </summary>
    /// <example>1000</example>
    public int MaxTokensPerSource { get; set; } = 1000;

    /// <summary>
    /// Maximum number of sources to gather per one search.
    /// </summary>
    /// <example>5</example>
    public int MaxSourceCountPerSearch { get; set; } = 5;
}
