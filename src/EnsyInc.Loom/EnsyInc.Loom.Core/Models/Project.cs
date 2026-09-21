namespace EnsyInc.Loom.Core.Models;

public sealed record Project : BaseModel
{
    public required string Name { get; init; }
}
