namespace DeepResearch.Core;

public class DeepResearchOptions
{
    /// <summary>
    /// Maximum number of research loops to perform.
    /// </summary>
    /// <example>3</example>
    public int MaxResearchLoops { get; set; } = 3;

    /// <summary>
    /// Maximum number of character to process per source.
    /// The overflowing characters will be truncated.
    /// </summary>
    /// <example>1000</example>
    public int MaxCharacterPerSource { get; set; } = 4000;

    /// <summary>
    /// Maximum number of sources to gather per one search.
    /// </summary>
    /// <example>5</example>
    public int MaxSourceCountPerSearch { get; set; } = 5;

    /// <summary>
    /// Flag to combine all the individual summaries to generate a final summary
    /// </summary>
    /// <example>true</example>
    public bool EnableSummaryConsolidation { get; set; } = false;

}
