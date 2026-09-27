using EnsyInc.Loom.Core.Errors;
using EnsyInc.Loom.Core.Models;
using EnsyInc.Loom.DataAccess.Abstractions;
using EnsyInc.Loom.DataAccess.Mappers;
using EnsyInc.Loom.DataAccess.Models;
using EnsyInc.Loom.Services.Abstractions;

using EnsyNet.Core.Results;
using EnsyNet.DataAccess.Abstractions.Errors;

namespace EnsyInc.Loom.Services.Implementations;

internal sealed class WorkItemTypeStatusesService(
    IWorkItemTypeStatusRepo typeStatusRepo,
    ITplWorkItemRepo typeRepo,
    ITplWorkItemStatusRepo statusRepo,
    IStatusTransitionRepo transitionRepo) : IWorkItemTypeStatusesService
{
    public async Task<Result<IEnumerable<TplWorkItemStatus>>> ListStatusesForType(Guid typeId, CancellationToken ct)
    {
        var links = await typeStatusRepo.GetManyByExpression(l => l.TypeId == typeId, ct);
        if (links.HasError)
        {
            return Result.FromError<IEnumerable<TplWorkItemStatus>>(new UnexpectedError());
        }

        var statusIds = links.Data.Select(l => l.StatusId).ToHashSet();
        var statuses = await statusRepo.GetManyByExpression(s => statusIds.Contains(s.Id), ct);

        return statuses.HasError
            ? Result.FromError<IEnumerable<TplWorkItemStatus>>(new UnexpectedError())
            : Result.Ok(statuses.Data.Select(e => e.ToCoreModel()));
    }

    public async Task<Result> AddStatusToType(Guid typeId, Guid statusId, CancellationToken ct)
    {
        var typeResult = await typeRepo.GetById(typeId, ct);
        if (typeResult.HasError)
        {
            return typeResult.Error switch
            {
                EntityNotFoundError<TplWorkItemEntity> => Result.FromError(new TplWorkItemNotFoundError()),
                _ => Result.FromError(new UnexpectedError()),
            };
        }

        var statusResult = await statusRepo.GetById(statusId, ct);
        if (statusResult.HasError)
        {
            return statusResult.Error switch
            {
                EntityNotFoundError<TplWorkItemStatusEntity> => Result.FromError(new TplWorkItemStatusNotFoundError()),
                _ => Result.FromError(new UnexpectedError()),
            };
        }

        var existingLink = await typeStatusRepo.GetManyByExpression(l => l.TypeId == typeId && l.StatusId == statusId, ct);
        if (existingLink.HasError)
        {
            return Result.FromError(new UnexpectedError());
        }

        if (existingLink.Data.Any())
        {
            return Result.Ok();
        }

        var link = new WorkItemTypeStatus { TypeId = typeId, StatusId = statusId };
        var insertResult = await typeStatusRepo.Insert(link.ToEntityModel(), ct);

        if (insertResult.HasError)
        {
            return insertResult.Error switch
            {
                UniqueConstraintViolationError => Result.Ok(),
                _ => Result.FromError(new UnexpectedError()),
            };
        }

        return Result.Ok();
    }

    public async Task<Result> RemoveStatusFromType(Guid typeId, Guid statusId, CancellationToken ct)
    {
        var existingLink = await typeStatusRepo.GetManyByExpression(l => l.TypeId == typeId && l.StatusId == statusId, ct);
        if (existingLink.HasError)
        {
            return Result.FromError(new UnexpectedError());
        }

        var link = existingLink.Data.FirstOrDefault();
        if (link is null)
        {
            return Result.Ok();
        }

        var typeResult = await typeRepo.GetById(typeId, ct);
        if (typeResult.HasError)
        {
            return typeResult.Error switch
            {
                EntityNotFoundError<TplWorkItemEntity> => Result.FromError(new TplWorkItemNotFoundError()),
                _ => Result.FromError(new UnexpectedError()),
            };
        }

        if (typeResult.Data.InitialStatusId == statusId)
        {
            return Result.FromError(new InitialStatusCannotBeRemovedError());
        }

        var transitions = await transitionRepo.GetManyByExpression(
            t => t.TypeId == typeId && (t.FromStatusId == statusId || t.ToStatusId == statusId), ct);
        if (transitions.HasError)
        {
            return Result.FromError(new UnexpectedError());
        }

        if (transitions.Data.Any())
        {
            return Result.FromError(new StatusHasTransitionsError());
        }

        var deleteResult = await typeStatusRepo.SoftDelete(link.Id, ct);

        if (deleteResult.HasError)
        {
            return deleteResult.Error switch
            {
                DeleteOperationFailedError => Result.Ok(),
                _ => Result.FromError(new UnexpectedError()),
            };
        }

        return Result.Ok();
    }
}
