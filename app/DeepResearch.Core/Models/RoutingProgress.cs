namespace DeepResearch.Core.Models;

public class RoutingProgress : ProgressBase
{
    public RoutingProgress() : base(ProgressTypes.Routing)
    {
    }

    public string Decision { get; set; } = string.Empty;
    public int LoopCount { get; set; }
}