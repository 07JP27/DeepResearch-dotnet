namespace DeepResearch.Core.Models;

public record DeepResearchOrchestratorArguments(string SessionId, string Topic, int MaxResearchLoops = 3);
