using DeepResearch.Core.Models;

namespace DeepResearch.DurableFunctions.SignalR;

public record NotifyProgressArguments(ProgressKey ProgressKey, ProgressEnvelope Progress);