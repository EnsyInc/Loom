using EnsyNet.Core.Results;

namespace EnsyInc.Loom.Core.Errors;

public sealed record StatusTransitionNotFoundError() : Error(ErrorCodes.StatusTransitionNotFoundError, "The status transition was not found.");

public sealed record StatusTransitionAlreadyExistsError(Guid ExistingTransitionId) : Error(ErrorCodes.StatusTransitionAlreadyExistsError, "This transition already exists for the work item type.");

public sealed record SameStatusTransitionError() : Error(ErrorCodes.SameStatusTransitionError, "A transition's from and to status must be different.");
