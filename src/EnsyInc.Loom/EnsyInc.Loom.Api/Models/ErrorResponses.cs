namespace EnsyInc.Loom.Api.Models;

internal static class ErrorResponses
{
    public static readonly ErrorResponse ProjectNotFoundError = new("ProjectNotFound", "The requested project was not found.", []);

    public static readonly ErrorResponse UnexpectedError = new("UnexpectedError", "An unexpected error occurred.", []);

    public static ErrorResponse ValidationError(Dictionary<string, string> parameters)
        => new("ValidationError", "One or more fields failed validation.", parameters);
}
