namespace DeepResearch.Core.Models;

public abstract class ProgressBase
{
    protected ProgressBase(string type)
    {
        Type = type;
    }

    public string Type { get; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string Step => Type;
}