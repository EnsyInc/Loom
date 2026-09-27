using EnsyNet.DataAccess.Abstractions.Models;

namespace EnsyInc.Loom.DataAccess.Models;

public sealed record StatusTransitionEntity : DbEntity
{
    public required Guid TypeId { get; init; }

    public required Guid FromStatusId { get; init; }

    public required Guid ToStatusId { get; init; }
}
