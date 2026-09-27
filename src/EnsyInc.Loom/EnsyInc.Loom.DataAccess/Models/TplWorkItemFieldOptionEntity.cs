using EnsyNet.DataAccess.Abstractions.Models;

namespace EnsyInc.Loom.DataAccess.Models;

public sealed record TplWorkItemFieldOptionEntity : DbEntity
{
    public required Guid FieldId { get; init; }

    public required string Value { get; init; }

    public required string Label { get; init; }

    public required int Rank { get; init; }
}
