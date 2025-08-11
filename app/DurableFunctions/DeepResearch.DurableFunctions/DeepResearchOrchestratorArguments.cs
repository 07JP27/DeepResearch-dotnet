namespace DeepResearch.DurableFunctions;

public record DeepResearchOrchestratorArguments(string Topic, int MaxResearchLoops = 3);
