using System.Collections.Concurrent;
using System.Text.Json;

namespace DeepResearch.Web.Services;

public class ResearchProgressService
{
    private readonly ConcurrentDictionary<string, List<ProgressMessage>> _progressStore = new();
    private readonly ConcurrentDictionary<string, bool> _researchStatus = new();

    public void AddProgress(string clientId, string type, object data)
    {
        var message = new ProgressMessage
        {
            Type = type,
            Data = JsonSerializer.SerializeToElement(data),
            Timestamp = DateTime.UtcNow
        };

        _progressStore.AddOrUpdate(clientId,
            new List<ProgressMessage> { message },
            (key, existing) =>
            {
                existing.Add(message);
                return existing;
            });
    }

    public List<ProgressMessage> GetProgress(string clientId, int fromIndex = 0)
    {
        if (_progressStore.TryGetValue(clientId, out var messages))
        {
            return messages.Skip(fromIndex).ToList();
        }
        return new List<ProgressMessage>();
    }

    public void SetResearchStatus(string clientId, bool isResearching)
    {
        _researchStatus.AddOrUpdate(clientId, isResearching, (key, existing) => isResearching);
    }

    public bool IsResearching(string clientId)
    {
        return _researchStatus.TryGetValue(clientId, out var status) && status;
    }

    public void ClearProgress(string clientId)
    {
        _progressStore.TryRemove(clientId, out _);
        _researchStatus.TryRemove(clientId, out _);
    }

    public int GetProgressCount(string clientId)
    {
        if (_progressStore.TryGetValue(clientId, out var messages))
        {
            return messages.Count;
        }
        return 0;
    }
}

public class ProgressMessage
{
    public string Type { get; set; } = "";
    public JsonElement Data { get; set; }
    public DateTime Timestamp { get; set; }
}
