using EnsyInc.Loom.Core.Errors;
using EnsyInc.Loom.Core.Models;
using EnsyInc.Loom.DataAccess.Abstractions;
using EnsyInc.Loom.DataAccess.Mappers;
using EnsyInc.Loom.DataAccess.Models;
using EnsyInc.Loom.Services.Abstractions;

using EnsyNet.Core.Results;
using EnsyNet.DataAccess.Abstractions.Errors;

namespace EnsyInc.Loom.Services.Implementations;

internal sealed class TplWorkItemStatusesService(ITplWorkItemStatusRepo statusRepo, IWorkItemTypeStatusRepo typeStatusRepo) : ITplWorkItemStatusesService
{
    public async Task<Result<IEnumerable<TplWorkItemStatus>>> ListStatuses(CancellationToken ct)
    {
        var result = await statusRepo.GetAll(ct);
        return result.HasError
            ? Result.FromError<IEnumerable<TplWorkItemStatus>>(new UnexpectedError())
            : Result.Ok(result.Data.Select(e => e.ToCoreModel()));
    }

    public async Task<Result<TplWorkItemStatus>> GetStatus(Guid id, CancellationToken ct)
    {
        var result = await statusRepo.GetById(id, ct);

        if (result.HasError)
        {
            return result.Error switch
            {
                EntityNotFoundError<TplWorkItemStatusEntity> => Result.FromError<TplWorkItemStatus>(new TplWorkItemStatusNotFoundError()),
                _ => Result.FromError<TplWorkItemStatus>(new UnexpectedError()),
            };
        }

        return Result.Ok(result.Data.ToCoreModel());
    }

    public async Task<Result<TplWorkItemStatus>> CreateStatus(TplWorkItemStatus status, CancellationToken ct)
    {
        var result = await statusRepo.Insert(status.ToEntityModel(), ct);

        if (result.HasError)
        {
            return result.Error switch
            {
                UniqueConstraintViolationError => await BuildNameAlreadyExistsError(status.Name, ct),
                _ => Result.FromError<TplWorkItemStatus>(new UnexpectedError()),
            };
        }

        return Result.Ok(result.Data.ToCoreModel());
    }

    public async Task<Result<TplWorkItemStatus>> UpdateStatus(TplWorkItemStatus status, CancellationToken ct)
    {
        var existing = await GetStatus(status.Id, ct);
        if (existing.HasError)
        {
            return Result.FromError<TplWorkItemStatus>(existing.Error);
        }

        var updateResult = await statusRepo.Update(status.Id, updates =>
        {
            updates.AddUpdate(s => s.Name, _ => status.Name);
            updates.AddUpdate(s => s.Category, _ => status.Category);
        }, ct);

        if (updateResult.HasError)
        {
            return updateResult.Error switch
            {
                UpdateOperationFailedError => Result.FromError<TplWorkItemStatus>(new TplWorkItemStatusNotFoundError()),
                UniqueConstraintViolationError => await BuildNameAlreadyExistsError(status.Name, ct),
                _ => Result.FromError<TplWorkItemStatus>(new UnexpectedError()),
            };
        }

        return Result.Ok(existing.Data with { Name = status.Name, Category = status.Category, UpdatedAt = DateTime.UtcNow });
    }

    public async Task<Result> SoftDeleteStatus(Guid id, CancellationToken ct)
    {
        var usages = await typeStatusRepo.GetManyByExpression(l => l.StatusId == id, ct);
        if (usages.HasError)
        {
            return Result.FromError(new UnexpectedError());
        }

        if (usages.Data.Any())
        {
            return Result.FromError(new TplWorkItemStatusInUseError());
        }

        var result = await statusRepo.SoftDelete(id, ct);

        if (result.HasError)
        {
            return result.Error switch
            {
                DeleteOperationFailedError => Result.Ok(),
                _ => Result.FromError(new UnexpectedError()),
            };
        }

        return Result.Ok();
    }

    private async Task<Result<TplWorkItemStatus>> BuildNameAlreadyExistsError(string name, CancellationToken ct)
    {
        var existing = await statusRepo.GetByExpression(s => s.Name == name, ct);

        return existing.HasError
            ? Result.FromError<TplWorkItemStatus>(new UnexpectedError())
            : Result.FromError<TplWorkItemStatus>(new TplWorkItemStatusNameAlreadyExistsError(existing.Data.Id));
    }
}
