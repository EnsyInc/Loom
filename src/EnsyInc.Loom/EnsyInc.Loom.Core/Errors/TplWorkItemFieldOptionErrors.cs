using EnsyNet.Core.Results;

namespace EnsyInc.Loom.Core.Errors;

public sealed record TplWorkItemFieldOptionNotFoundError() : Error(ErrorCodes.TplWorkItemFieldOptionNotFoundError, "The field option was not found.");

public sealed record TplWorkItemFieldOptionValueAlreadyExistsError(Guid ExistingOptionId) : Error(ErrorCodes.TplWorkItemFieldOptionValueAlreadyExistsError, "An option with this value already exists on the field.");

public sealed record FieldDoesNotSupportOptionsError() : Error(ErrorCodes.FieldDoesNotSupportOptionsError, "Only Option and MultiOption fields can have options.");
