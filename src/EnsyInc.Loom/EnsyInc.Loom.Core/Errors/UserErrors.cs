using EnsyNet.Core.Results;

namespace EnsyInc.Loom.Core.Errors;

public sealed record UserNotFoundError() : Error(ErrorCodes.UserNotFoundError, "The user was not found.");
