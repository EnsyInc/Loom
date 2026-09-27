namespace EnsyInc.Loom.Core.Models;

/// <summary>
/// A work item type: a shared definition used by reference. Projects opt into
/// a type via <see cref="ProjectWorkItemType"/>.
/// </summary>
public sealed record TplWorkItem : BaseModel
{
    public required string Name { get; init; }

    public string? IconUrl { get; init; }

    /// <summary>
    /// The status new items of this type start in. Null until the type has
    /// been configured with at least one status via
    /// <see cref="WorkItemTypeStatus"/>.
    /// </summary>
    public Guid? InitialStatusId { get; init; }
}
