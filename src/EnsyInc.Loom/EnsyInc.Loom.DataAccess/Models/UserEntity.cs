using EnsyNet.DataAccess.Abstractions.Models;

namespace EnsyInc.Loom.DataAccess.Models;

public sealed record UserEntity : DbEntity
{
    public required string EntraObjectId { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required string Email { get; init; }
}
