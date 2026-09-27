namespace EnsyInc.Loom.Core.Models;

public enum WorkItemFieldDataType
{
    /// <summary>
    /// A free-text value.
    /// </summary>
    Text = 0,

    /// <summary>
    /// A numeric value.
    /// </summary>
    Number = 1,

    /// <summary>
    /// A date value.
    /// </summary>
    Date = 2,

    /// <summary>
    /// A date and time value.
    /// </summary>
    DateTime = 3,

    /// <summary>
    /// A true/false value.
    /// </summary>
    Bool = 4,

    /// <summary>
    /// A single value chosen from the field's list of options.
    /// </summary>
    Option = 5,

    /// <summary>
    /// One or more values chosen from the field's list of options.
    /// </summary>
    MultiOption = 6,

    /// <summary>
    /// A reference to a user.
    /// </summary>
    User = 7,
}
