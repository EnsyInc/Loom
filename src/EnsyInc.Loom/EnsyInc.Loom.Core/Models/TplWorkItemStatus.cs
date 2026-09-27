namespace EnsyInc.Loom.Core.Models;

/// <summary>
/// A status a work item type's workflow can use.
/// A type opts into the statuses it uses via <see cref="WorkItemTypeStatus"/>.
/// </summary>
public sealed record TplWorkItemStatus : BaseModel
{
    public required string Name { get; init; }

    public required StatusCategory Category { get; init; }
}
