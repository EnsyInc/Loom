namespace EnsyInc.Loom.Core.Models;

/// <summary>
/// An allowed move between two statuses for a work item type's workflow.
/// </summary>
public sealed record StatusTransition : BaseModel
{
    public required Guid TypeId { get; init; }

    public required Guid FromStatusId { get; init; }

    public required Guid ToStatusId { get; init; }
}
