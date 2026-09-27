using EnsyNet.Core.Results;

namespace EnsyInc.Loom.Core.Errors;

public sealed record TplWorkItemFieldNotFoundError() : Error(ErrorCodes.TplWorkItemFieldNotFoundError, "The field was not found.");

public sealed record TplWorkItemFieldKeyAlreadyExistsError(Guid ExistingFieldId) : Error(ErrorCodes.TplWorkItemFieldKeyAlreadyExistsError, "A field with this key already exists on the work item type.");

public sealed record TplWorkItemFieldInUseError() : Error(ErrorCodes.TplWorkItemFieldInUseError, "The field still has options and cannot be deleted.");
