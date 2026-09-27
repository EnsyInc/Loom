namespace EnsyInc.Loom.Api.Models;

internal static class ErrorResponses
{
    public static readonly ErrorResponse ProjectNotFoundError = new("ProjectNotFound", "The requested project was not found.", []);

    public static readonly ErrorResponse TplWorkItemStatusNotFoundError = new("TplWorkItemStatusNotFound", "The requested status was not found.", []);

    public static readonly ErrorResponse TplWorkItemStatusInUseError = new("TplWorkItemStatusInUse", "The status is used by at least one work item type and cannot be deleted.", []);

    public static readonly ErrorResponse TplWorkItemNotFoundError = new("TplWorkItemNotFound", "The requested work item type was not found.", []);

    public static readonly ErrorResponse TplWorkItemInUseError = new("TplWorkItemInUse", "The work item type is used by at least one project and cannot be deleted.", []);

    public static readonly ErrorResponse StatusNotInTypeWorkflowError = new("StatusNotInTypeWorkflow", "The status is not one of the work item type's statuses.", []);

    public static readonly ErrorResponse InitialStatusCannotBeRemovedError = new("InitialStatusCannotBeRemoved", "The status is the work item type's initial status and cannot be removed from it.", []);

    public static readonly ErrorResponse StatusHasTransitionsError = new("StatusHasTransitions", "The status still has transitions to or from it for this work item type.", []);

    public static readonly ErrorResponse SameStatusTransitionError = new("SameStatusTransition", "A transition's from and to status must be different.", []);

    public static readonly ErrorResponse TplWorkItemFieldNotFoundError = new("TplWorkItemFieldNotFound", "The requested field was not found.", []);

    public static readonly ErrorResponse TplWorkItemFieldInUseError = new("TplWorkItemFieldInUse", "The field still has options and cannot be deleted.", []);

    public static readonly ErrorResponse TplWorkItemFieldOptionNotFoundError = new("TplWorkItemFieldOptionNotFound", "The requested field option was not found.", []);

    public static readonly ErrorResponse FieldDoesNotSupportOptionsError = new("FieldDoesNotSupportOptions", "Only Option and MultiOption fields can have options.", []);

    public static readonly ErrorResponse UnexpectedError = new("UnexpectedError", "An unexpected error occurred.", []);

    public static ErrorResponse ValidationError(Dictionary<string, string> parameters)
        => new("ValidationError", "One or more fields failed validation.", parameters);

    public static ErrorResponse TplWorkItemStatusNameAlreadyExistsError(Guid existingStatusId)
        => new("TplWorkItemStatusNameAlreadyExists", "A status with this name already exists.", new Dictionary<string, string> { ["ExistingStatusId"] = existingStatusId.ToString() });

    public static ErrorResponse TplWorkItemNameAlreadyExistsError(Guid existingTypeId)
        => new("TplWorkItemNameAlreadyExists", "A work item type with this name already exists.", new Dictionary<string, string> { ["ExistingTypeId"] = existingTypeId.ToString() });

    public static ErrorResponse StatusTransitionAlreadyExistsError(Guid existingTransitionId)
        => new("StatusTransitionAlreadyExists", "This transition already exists for the work item type.", new Dictionary<string, string> { ["ExistingTransitionId"] = existingTransitionId.ToString() });

    public static ErrorResponse TplWorkItemFieldKeyAlreadyExistsError(Guid existingFieldId)
        => new("TplWorkItemFieldKeyAlreadyExists", "A field with this key already exists on the work item type.", new Dictionary<string, string> { ["ExistingFieldId"] = existingFieldId.ToString() });

    public static ErrorResponse TplWorkItemFieldOptionValueAlreadyExistsError(Guid existingOptionId)
        => new("TplWorkItemFieldOptionValueAlreadyExists", "An option with this value already exists on the field.", new Dictionary<string, string> { ["ExistingOptionId"] = existingOptionId.ToString() });
}
