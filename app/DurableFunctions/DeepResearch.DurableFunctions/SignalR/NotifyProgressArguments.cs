using DeepResearch.Core.Models;

namespace DeepResearch.DurableFunctions.SignalR;

public record NotifyProgressArguments(string SessionId, ProgressEnvelope Progress);