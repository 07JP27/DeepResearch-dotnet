namespace DeepResearch.Core.Models;

public class ErrorProgress : ProgressBase
{
    public ErrorProgress() : base(ProgressTypes.Error)
    {
    }

    public string Message { get; set; } = string.Empty;
}