namespace DeepResearch.SearchClient.Tavily;

/// <summary>
/// Exception thrown when an invalid API key is provided to Tavily
/// </summary>
public class TavilyInvalidApiKeyException : Exception
{
    public TavilyInvalidApiKeyException(string? message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when the API key is missing
/// </summary>
public class TavilyMissingApiKeyException : Exception
{
    public TavilyMissingApiKeyException() : base("No API key provided. Please provide the api_key attribute or set the TAVILY_API_KEY environment variable.")
    {
    }
}

/// <summary>
/// Exception thrown when the usage limit is exceeded
/// </summary>
public class TavilyUsageLimitExceededException : Exception
{
    public TavilyUsageLimitExceededException(string? message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown for bad requests
/// </summary>
public class TavilyBadRequestException : Exception
{
    public TavilyBadRequestException(string? message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown for forbidden requests
/// </summary>
public class TavilyForbiddenException : Exception
{
    public TavilyForbiddenException(string? message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when a request times out
/// </summary>
public class TavilyTimeoutException : Exception
{
    public TavilyTimeoutException(int timeoutSeconds) : base($"Request timed out after {timeoutSeconds} seconds.")
    {
    }
}
