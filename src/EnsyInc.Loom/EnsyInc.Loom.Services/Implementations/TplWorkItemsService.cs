using EnsyInc.Loom.Core.Errors;
using EnsyInc.Loom.Core.Models;
using EnsyInc.Loom.DataAccess.Abstractions;
using EnsyInc.Loom.DataAccess.Mappers;
using EnsyInc.Loom.DataAccess.Models;
using EnsyInc.Loom.Services.Abstractions;

using EnsyNet.Core.Results;
using EnsyNet.DataAccess.Abstractions.Errors;

namespace EnsyInc.Loom.Services.Implementations;

internal sealed class TplWorkItemsService(
    ITplWorkItemRepo typeRepo,
    IWorkItemTypeStatusRepo typeStatusRepo,
    IProjectWorkItemTypeRepo projectTypeRepo,
    IStatusTransitionRepo transitionRepo,
    IUnitOfWork unitOfWork) : ITplWorkItemsService
{
    public async Task<Result<IEnumerable<TplWorkItem>>> ListTypes(CancellationToken ct)
    {
        var result = await typeRepo.GetAll(ct);
        return result.HasError
            ? Result.FromError<IEnumerable<TplWorkItem>>(new UnexpectedError())
            : Result.Ok(result.Data.Select(e => e.ToCoreModel()));
    }

    public async Task<Result<TplWorkItem>> GetType(Guid id, CancellationToken ct)
    {
        var result = await typeRepo.GetById(id, ct);

        if (result.HasError)
        {
            return result.Error switch
            {
                EntityNotFoundError<TplWorkItemEntity> => Result.FromError<TplWorkItem>(new TplWorkItemNotFoundError()),
                _ => Result.FromError<TplWorkItem>(new UnexpectedError()),
            };
        }

        return Result.Ok(result.Data.ToCoreModel());
    }

    public async Task<Result<TplWorkItem>> CreateType(TplWorkItem type, CancellationToken ct)
    {
        var toInsert = type with { InitialStatusId = null };
        var result = await typeRepo.Insert(toInsert.ToEntityModel(), ct);

        if (result.HasError)
        {
            return result.Error switch
            {
                UniqueConstraintViolationError => await BuildNameAlreadyExistsError(type.Name, ct),
                _ => Result.FromError<TplWorkItem>(new UnexpectedError()),
            };
        }

        return Result.Ok(result.Data.ToCoreModel());
    }

    public async Task<Result<TplWorkItem>> UpdateType(TplWorkItem type, CancellationToken ct)
    {
        var existing = await GetType(type.Id, ct);
        if (existing.HasError)
        {
            return Result.FromError<TplWorkItem>(existing.Error);
        }

        var updateResult = await typeRepo.Update(type.Id, updates =>
        {
            updates.AddUpdate(t => t.Name, _ => type.Name);
            updates.AddUpdate(t => t.IconUrl, _ => type.IconUrl);
        }, ct);

        if (updateResult.HasError)
        {
            return updateResult.Error switch
            {
                UpdateOperationFailedError => Result.FromError<TplWorkItem>(new TplWorkItemNotFoundError()),
                UniqueConstraintViolationError => await BuildNameAlreadyExistsError(type.Name, ct),
                _ => Result.FromError<TplWorkItem>(new UnexpectedError()),
            };
        }

        return Result.Ok(existing.Data with { Name = type.Name, IconUrl = type.IconUrl, UpdatedAt = DateTime.UtcNow });
    }

    public async Task<Result<TplWorkItem>> SetInitialStatus(Guid typeId, Guid statusId, CancellationToken ct)
    {
        var existing = await GetType(typeId, ct);
        if (existing.HasError)
        {
            return Result.FromError<TplWorkItem>(existing.Error);
        }

        var membership = await typeStatusRepo.GetManyByExpression(l => l.TypeId == typeId && l.StatusId == statusId, ct);
        if (membership.HasError)
        {
            return Result.FromError<TplWorkItem>(new UnexpectedError());
        }

        if (!membership.Data.Any())
        {
            return Result.FromError<TplWorkItem>(new StatusNotInTypeWorkflowError());
        }

        var updateResult = await typeRepo.Update(typeId, updates => updates.AddUpdate(t => t.InitialStatusId, _ => statusId), ct);

        if (updateResult.HasError)
        {
            return updateResult.Error switch
            {
                UpdateOperationFailedError => Result.FromError<TplWorkItem>(new TplWorkItemNotFoundError()),
                _ => Result.FromError<TplWorkItem>(new UnexpectedError()),
            };
        }

        return Result.Ok(existing.Data with { InitialStatusId = statusId, UpdatedAt = DateTime.UtcNow });
    }

    public Task<Result> SoftDeleteType(Guid id, CancellationToken ct)
        => unitOfWork.RunInTransaction(async () =>
        {
            var usages = await projectTypeRepo.GetManyByExpression(l => l.TypeId == id, ct);
            if (usages.HasError)
            {
                return Result.FromError(new UnexpectedError());
            }

            if (usages.Data.Any())
            {
                return Result.FromError(new TplWorkItemInUseError());
            }

            var result = await typeRepo.SoftDelete(id, ct);

            if (result.HasError)
            {
                return result.Error switch
                {
                    DeleteOperationFailedError => Result.Ok(),
                    _ => Result.FromError(new UnexpectedError()),
                };
            }

            // Also removes the type's status/transition links, so a status it used isn't left
            // permanently blocked from deletion by a link to a type that no longer exists.
            var transitionsResult = await transitionRepo.SoftDelete(t => t.TypeId == id, ct);
            if (transitionsResult.HasError && transitionsResult.Error is not BulkDeleteOperationFailedError)
            {
                return Result.FromError(new UnexpectedError());
            }

            var statusLinksResult = await typeStatusRepo.SoftDelete(l => l.TypeId == id, ct);
            if (statusLinksResult.HasError && statusLinksResult.Error is not BulkDeleteOperationFailedError)
            {
                return Result.FromError(new UnexpectedError());
            }

            return Result.Ok();
        }, ct);

    private async Task<Result<TplWorkItem>> BuildNameAlreadyExistsError(string name, CancellationToken ct)
    {
        var existing = await typeRepo.GetByExpression(t => t.Name == name, ct);

        return existing.HasError
            ? Result.FromError<TplWorkItem>(new UnexpectedError())
            : Result.FromError<TplWorkItem>(new TplWorkItemNameAlreadyExistsError(existing.Data.Id));
    }
}
