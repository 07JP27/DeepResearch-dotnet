namespace DeepResearch.SearchClient;

/// <summary>
/// Base exception for search client operations
/// </summary>
public abstract class SearchClientException : Exception
{
    protected SearchClientException(string? message) : base(message)
    {
    }

    protected SearchClientException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when an invalid API key is provided
/// </summary>
public class InvalidApiKeyException : SearchClientException
{
    public InvalidApiKeyException(string? message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when the API key is missing
/// </summary>
public class MissingApiKeyException : SearchClientException
{
    public MissingApiKeyException(string message = "No API key provided. Please provide the API key or set the appropriate environment variable.") : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when the usage limit is exceeded
/// </summary>
public class UsageLimitExceededException : SearchClientException
{
    public UsageLimitExceededException(string? message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown for bad requests
/// </summary>
public class BadRequestException : SearchClientException
{
    public BadRequestException(string? message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown for forbidden requests
/// </summary>
public class ForbiddenException : SearchClientException
{
    public ForbiddenException(string? message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when a request times out
/// </summary>
public class RequestTimeoutException : SearchClientException
{
    public RequestTimeoutException(int timeoutSeconds) : base($"Request timed out after {timeoutSeconds} seconds.")
    {
    }
}
