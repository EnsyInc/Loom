using EnsyNet.DataAccess.Abstractions.Models;

namespace EnsyInc.Loom.DataAccess.Models;

public sealed record ProjectEntity : DbEntity
{
    public required string Name { get; init; }
}
