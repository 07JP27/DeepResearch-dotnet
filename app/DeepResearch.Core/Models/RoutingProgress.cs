namespace DeepResearch.Core.Models;

public class RoutingProgress : ProgressBase
{
    public RoutingProgress() : base(ProgressTypes.Routing)
    {
    }

    public RoutingDecision Decision { get; set; } = RoutingDecision.Continue;
    public int LoopCount { get; set; }
}

public enum RoutingDecision
{
    Continue,
    RetrySearch,
    Finalize
}