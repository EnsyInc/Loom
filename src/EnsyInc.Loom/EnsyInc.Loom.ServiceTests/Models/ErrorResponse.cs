namespace EnsyInc.Loom.ServiceTests.Models;

public sealed record ErrorResponse(
    string ErrorCode,
    string ErrorMessage,
    Dictionary<string, string> Parameters);
