using EnsyInc.Loom.Core.Errors;
using EnsyInc.Loom.Core.Models;
using EnsyInc.Loom.DataAccess.Abstractions;
using EnsyInc.Loom.DataAccess.Mappers;
using EnsyInc.Loom.DataAccess.Models;
using EnsyInc.Loom.Services.Abstractions;

using EnsyNet.Core.Results;
using EnsyNet.DataAccess.Abstractions.Errors;

namespace EnsyInc.Loom.Services.Implementations;

internal sealed class UsersService(IUserRepo userRepo) : IUsersService
{
    public async Task<Result<User>> GetByEntraObjectId(string entraObjectId, CancellationToken ct)
    {
        var result = await userRepo.GetByExpression(u => u.EntraObjectId == entraObjectId, ct);

        if (result.HasError)
        {
            return result.Error switch
            {
                EntityNotFoundError<UserEntity> => Result.FromError<User>(new UserNotFoundError()),
                _ => Result.FromError<User>(new UnexpectedError()),
            };
        }

        return Result.Ok(result.Data.ToCoreModel());
    }

    public async Task<Result<User>> UpsertOnLogin(string entraObjectId, string firstName, string lastName, string email, CancellationToken ct)
    {
        var existing = await GetByEntraObjectId(entraObjectId, ct);

        if (existing.HasError)
        {
            return existing.Error is UserNotFoundError
                ? await CreateUser(entraObjectId, firstName, lastName, email, ct)
                : Result.FromError<User>(existing.Error);
        }

        var updateResult = await userRepo.Update(existing.Data.Id, updates =>
        {
            updates.AddUpdate(u => u.FirstName, _ => firstName);
            updates.AddUpdate(u => u.LastName, _ => lastName);
            updates.AddUpdate(u => u.Email, _ => email);
        }, ct);

        if (updateResult.HasError)
        {
            return updateResult.Error switch
            {
                UpdateOperationFailedError => Result.FromError<User>(new UserNotFoundError()),
                _ => Result.FromError<User>(new UnexpectedError()),
            };
        }

        return Result.Ok(existing.Data with { FirstName = firstName, LastName = lastName, Email = email, UpdatedAt = DateTime.UtcNow });
    }

    private async Task<Result<User>> CreateUser(string entraObjectId, string firstName, string lastName, string email, CancellationToken ct)
    {
        var user = new User { EntraObjectId = entraObjectId, FirstName = firstName, LastName = lastName, Email = email };
        var insertResult = await userRepo.Insert(user.ToEntityModel(), ct);

        if (insertResult.HasError)
        {
            return insertResult.Error switch
            {
                // Two concurrent first-logins for the same entraObjectId: the loser just reads back
                // the winner's row instead of failing, since both requests represent the same user.
                UniqueConstraintViolationError => await GetByEntraObjectId(entraObjectId, ct),
                _ => Result.FromError<User>(new UnexpectedError()),
            };
        }

        return Result.Ok(insertResult.Data.ToCoreModel());
    }
}
