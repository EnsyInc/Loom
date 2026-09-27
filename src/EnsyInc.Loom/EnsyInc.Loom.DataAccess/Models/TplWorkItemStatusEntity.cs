using EnsyInc.Loom.Core.Models;

using EnsyNet.DataAccess.Abstractions.Models;

namespace EnsyInc.Loom.DataAccess.Models;

public sealed record TplWorkItemStatusEntity : DbEntity
{
    public required string Name { get; init; }

    public required StatusCategory Category { get; init; }
}
