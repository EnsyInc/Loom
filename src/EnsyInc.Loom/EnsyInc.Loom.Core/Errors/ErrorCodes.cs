namespace EnsyInc.Loom.Core.Errors;

public static class ErrorCodes
{
    #region Generic Errors

    public const string UnexpectedError = "[UnexpectedError]";

    #endregion

    #region Projects

    public const string ProjectNotFoundError = "[ProjectNotFoundError]";

    #endregion

    #region TplWorkItemStatuses

    public const string TplWorkItemStatusNotFoundError = "[TplWorkItemStatusNotFoundError]";
    public const string TplWorkItemStatusNameAlreadyExistsError = "[TplWorkItemStatusNameAlreadyExistsError]";
    public const string TplWorkItemStatusInUseError = "[TplWorkItemStatusInUseError]";

    #endregion

    #region TplWorkItems

    public const string TplWorkItemNotFoundError = "[TplWorkItemNotFoundError]";
    public const string TplWorkItemNameAlreadyExistsError = "[TplWorkItemNameAlreadyExistsError]";
    public const string TplWorkItemInUseError = "[TplWorkItemInUseError]";

    #endregion

    #region WorkItemTypeStatuses

    public const string StatusNotInTypeWorkflowError = "[StatusNotInTypeWorkflowError]";
    public const string InitialStatusCannotBeRemovedError = "[InitialStatusCannotBeRemovedError]";
    public const string StatusHasTransitionsError = "[StatusHasTransitionsError]";

    #endregion

    #region StatusTransitions

    public const string StatusTransitionNotFoundError = "[StatusTransitionNotFoundError]";
    public const string StatusTransitionAlreadyExistsError = "[StatusTransitionAlreadyExistsError]";
    public const string SameStatusTransitionError = "[SameStatusTransitionError]";

    #endregion

    #region TplWorkItemFields

    public const string TplWorkItemFieldNotFoundError = "[TplWorkItemFieldNotFoundError]";
    public const string TplWorkItemFieldKeyAlreadyExistsError = "[TplWorkItemFieldKeyAlreadyExistsError]";
    public const string TplWorkItemFieldInUseError = "[TplWorkItemFieldInUseError]";

    #endregion

    #region TplWorkItemFieldOptions

    public const string TplWorkItemFieldOptionNotFoundError = "[TplWorkItemFieldOptionNotFoundError]";
    public const string TplWorkItemFieldOptionValueAlreadyExistsError = "[TplWorkItemFieldOptionValueAlreadyExistsError]";
    public const string FieldDoesNotSupportOptionsError = "[FieldDoesNotSupportOptionsError]";

    #endregion

    #region Users

    public const string UserNotFoundError = "[UserNotFoundError]";

    #endregion
}
