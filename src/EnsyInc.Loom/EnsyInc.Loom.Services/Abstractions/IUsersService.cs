using EnsyInc.Loom.Core.Models;

using EnsyNet.Core.Results;

namespace EnsyInc.Loom.Services.Abstractions;

public interface IUsersService
{
    /// <summary>Looks up a user by their Entra <c>oid</c> claim. Fails with <see cref="Core.Errors.UserNotFoundError"/> if none has signed in yet.</summary>
    public Task<Result<User>> GetByEntraObjectId(string entraObjectId, CancellationToken ct);

    /// <summary>Provisions a user on first sign-in, or refreshes their profile fields if they already exist. There is no separate register endpoint.</summary>
    public Task<Result<User>> UpsertOnLogin(string entraObjectId, string firstName, string lastName, string email, CancellationToken ct);
}
