using EnsyNet.DataAccess.Abstractions.Models;

namespace EnsyInc.Loom.DataAccess.Models;

public sealed record ProjectWorkItemTypeEntity : DbEntity
{
    public required Guid ProjectId { get; init; }

    public required Guid TypeId { get; init; }
}
