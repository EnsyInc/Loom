using EnsyInc.Loom.Core.Models;

using EnsyNet.DataAccess.Abstractions.Models;

namespace EnsyInc.Loom.DataAccess.Models;

public sealed record TplWorkItemFieldEntity : DbEntity
{
    public required Guid TypeId { get; init; }

    public required string Key { get; init; }

    public required string Label { get; init; }

    public required WorkItemFieldDataType DataType { get; init; }

    public required bool Required { get; init; }

    public string? DefaultValue { get; init; }
}
