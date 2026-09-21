namespace EnsyInc.Loom.Api.Exceptions;

/// <summary>
/// Thrown when a controller's result switch reaches an error case it doesn't explicitly handle,
/// i.e. a service returned an <c>Error</c> subtype the controller wasn't written to expect.
/// </summary>
public sealed class UnhandledResultErrorException : Exception
{
    public UnhandledResultErrorException() : base("Unexpected error occurred.")
    {
    }

    public UnhandledResultErrorException(string message) : base(message)
    {
    }

    public UnhandledResultErrorException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
