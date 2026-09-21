using JetBrains.Annotations;

namespace EnsyInc.Loom.Api.Models;

/// <summary>Describes a request that failed.</summary>
/// <param name="ErrorCode">A stable machine-readable code identifying the error, e.g. <c>ProjectNotFound</c>.</param>
/// <param name="ErrorMessage">A human-readable description of what went wrong.</param>
/// <param name="Parameters">Additional structured context about the error, if any.</param>
[PublicAPI]
public sealed record ErrorResponse(
    string ErrorCode,
    string ErrorMessage,
    Dictionary<string, string> Parameters);
