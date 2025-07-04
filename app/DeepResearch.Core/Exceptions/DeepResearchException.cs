namespace DeepResearch.Core.Exceptions;

using System;

public class DeepResearchException : Exception
{
    public string Step { get; set; }
    public DeepResearchException(string message, string step, Exception innerException = null)
        : base(message, innerException)
    {
        Step = step;
    }
}
