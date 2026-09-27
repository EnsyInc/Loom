using EnsyNet.DataAccess.Abstractions.Models;

namespace EnsyInc.Loom.DataAccess.Models;

public sealed record TplWorkItemEntity : DbEntity
{
    public required string Name { get; init; }

    public string? IconUrl { get; init; }

    public Guid? InitialStatusId { get; init; }
}
