using EnsyNet.Core.Results;

namespace EnsyInc.Loom.Core.Errors;

public sealed record ProjectNotFoundError() : Error(ErrorCodes.ProjectNotFoundError, "The project was not found.");
