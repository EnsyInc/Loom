namespace EnsyInc.Loom.ServiceTests.Models;

// Deliberately independent copies of the Api/Core enums (not a shared reference) so a breaking
// wire-contract change (renamed/removed member) fails deserialization here instead of silently
// staying in sync because it's literally the same type. See Models/ErrorResponse.cs for the same
// reasoning applied to the response/request records.

public enum StatusCategory
{
    ToDo = 0,
    InProgress = 1,
    Done = 2,
}

public enum WorkItemFieldDataType
{
    Text = 0,
    Number = 1,
    Date = 2,
    DateTime = 3,
    Bool = 4,
    Option = 5,
    MultiOption = 6,
    User = 7,
}
