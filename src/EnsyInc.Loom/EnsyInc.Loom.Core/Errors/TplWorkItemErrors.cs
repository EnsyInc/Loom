using EnsyNet.Core.Results;

namespace EnsyInc.Loom.Core.Errors;

public sealed record TplWorkItemNotFoundError() : Error(ErrorCodes.TplWorkItemNotFoundError, "The work item type was not found.");

public sealed record TplWorkItemNameAlreadyExistsError(Guid ExistingTypeId) : Error(ErrorCodes.TplWorkItemNameAlreadyExistsError, "A work item type with this name already exists.");

public sealed record TplWorkItemInUseError() : Error(ErrorCodes.TplWorkItemInUseError, "The work item type is used by at least one project and cannot be deleted.");
