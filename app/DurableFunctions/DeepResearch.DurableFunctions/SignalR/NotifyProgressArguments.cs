using DeepResearch.Core.Models;

namespace DeepResearch.DurableFunctions.SignalR;

public record NotifyProgressArguments(string UserId, ProgressEnvelope Progress);