namespace EnsyInc.Loom.Core.Models;

/// <summary>
/// A project's opt-in to a work item type.
/// </summary>
public sealed record ProjectWorkItemType : BaseModel
{
    public required Guid ProjectId { get; init; }

    public required Guid TypeId { get; init; }
}
