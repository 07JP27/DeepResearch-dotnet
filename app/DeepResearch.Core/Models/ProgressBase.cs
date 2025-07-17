namespace DeepResearch.Core.Models;

public abstract class ProgressBase(string type)
{
    public string Type => type;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string Step => Type;
}