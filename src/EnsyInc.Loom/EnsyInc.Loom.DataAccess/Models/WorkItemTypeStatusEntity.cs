using EnsyNet.DataAccess.Abstractions.Models;

namespace EnsyInc.Loom.DataAccess.Models;

public sealed record WorkItemTypeStatusEntity : DbEntity
{
    public required Guid TypeId { get; init; }

    public required Guid StatusId { get; init; }
}
