using EnsyNet.Core.Results;

namespace EnsyInc.Loom.Core.Errors;

public sealed record TplWorkItemStatusNotFoundError() : Error(ErrorCodes.TplWorkItemStatusNotFoundError, "The status was not found.");

public sealed record TplWorkItemStatusNameAlreadyExistsError(Guid ExistingStatusId) : Error(ErrorCodes.TplWorkItemStatusNameAlreadyExistsError, "A status with this name already exists.");

public sealed record TplWorkItemStatusInUseError() : Error(ErrorCodes.TplWorkItemStatusInUseError, "The status is used by at least one work item type and cannot be deleted.");
