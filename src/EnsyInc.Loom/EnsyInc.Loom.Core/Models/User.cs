namespace EnsyInc.Loom.Core.Models;

public sealed record User : BaseModel
{
    public required string EntraObjectId { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required string Email { get; init; }
}
