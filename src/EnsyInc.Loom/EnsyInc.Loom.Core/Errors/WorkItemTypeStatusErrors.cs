using EnsyNet.Core.Results;

namespace EnsyInc.Loom.Core.Errors;

public sealed record StatusNotInTypeWorkflowError() : Error(ErrorCodes.StatusNotInTypeWorkflowError, "The status is not one of the work item type's statuses.");

public sealed record InitialStatusCannotBeRemovedError() : Error(ErrorCodes.InitialStatusCannotBeRemovedError, "The status is the work item type's initial status and cannot be removed from it.");

public sealed record StatusHasTransitionsError() : Error(ErrorCodes.StatusHasTransitionsError, "The status still has transitions to or from it for this work item type.");
