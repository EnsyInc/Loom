namespace EnsyInc.Loom.Core.Models;

/// <summary>
/// An allowed value of an <see cref="WorkItemFieldDataType.Option"/> or
/// <see cref="WorkItemFieldDataType.MultiOption"/> field.
/// </summary>
public sealed record TplWorkItemFieldOption : BaseModel
{
    public required Guid FieldId { get; init; }

    public required string Value { get; init; }

    public required string Label { get; init; }

    /// <summary>
    /// Options are ordered by this field.
    /// </summary>
    public required int Rank { get; init; }
}
