namespace DeepResearch.Core.Models;

public class ErrorProgress() : ProgressBase(ProgressTypes.Error)
{
    public string Message { get; set; } = string.Empty;
}