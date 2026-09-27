namespace EnsyInc.Loom.Core.Models;

/// <summary>
/// A work item type's opt-in to a status.
/// </summary>
public sealed record WorkItemTypeStatus : BaseModel
{
    public required Guid TypeId { get; init; }

    public required Guid StatusId { get; init; }
}
