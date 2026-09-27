namespace EnsyInc.Loom.Core.Models;

/// <summary>
/// A custom field defined on a work item type.
/// </summary>
public sealed record TplWorkItemField : BaseModel
{
    public required Guid TypeId { get; init; }

    /// <summary>
    /// Stable identifier for the field.
    /// </summary>
    public required string Key { get; init; }

    public required string Label { get; init; }

    public required WorkItemFieldDataType DataType { get; init; }

    public required bool Required { get; init; }

    /// <summary>
    /// Raw JSON default value, or null for no default.
    /// </summary>
    public string? DefaultValue { get; init; }
}
