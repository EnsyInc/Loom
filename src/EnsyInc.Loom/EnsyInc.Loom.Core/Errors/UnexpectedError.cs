using EnsyNet.Core.Results;

namespace EnsyInc.Loom.Core.Errors;

public sealed record UnexpectedError() : Error(ErrorCodes.UnexpectedError, "An unexpected error occurred.");
