using EnsyInc.Loom.Core.Errors;
using EnsyInc.Loom.Core.Models;
using EnsyInc.Loom.DataAccess.Abstractions;
using EnsyInc.Loom.DataAccess.Mappers;
using EnsyInc.Loom.DataAccess.Models;
using EnsyInc.Loom.Services.Abstractions;

using EnsyNet.Core.Results;
using EnsyNet.DataAccess.Abstractions.Errors;

namespace EnsyInc.Loom.Services.Implementations;

internal sealed class StatusTransitionsService(
    IStatusTransitionRepo transitionRepo,
    ITplWorkItemRepo typeRepo,
    IWorkItemTypeStatusRepo typeStatusRepo) : IStatusTransitionsService
{
    public async Task<Result<IEnumerable<StatusTransition>>> ListTransitionsForType(Guid typeId, CancellationToken ct)
    {
        var result = await transitionRepo.GetManyByExpression(t => t.TypeId == typeId, ct);
        return result.HasError
            ? Result.FromError<IEnumerable<StatusTransition>>(new UnexpectedError())
            : Result.Ok(result.Data.Select(e => e.ToCoreModel()));
    }

    public async Task<Result<StatusTransition>> CreateTransition(StatusTransition transition, CancellationToken ct)
    {
        if (transition.FromStatusId == transition.ToStatusId)
        {
            return Result.FromError<StatusTransition>(new SameStatusTransitionError());
        }

        var typeResult = await typeRepo.GetById(transition.TypeId, ct);
        if (typeResult.HasError)
        {
            return typeResult.Error switch
            {
                EntityNotFoundError<TplWorkItemEntity> => Result.FromError<StatusTransition>(new TplWorkItemNotFoundError()),
                _ => Result.FromError<StatusTransition>(new UnexpectedError()),
            };
        }

        var membership = await typeStatusRepo.GetManyByExpression(
            l => l.TypeId == transition.TypeId && (l.StatusId == transition.FromStatusId || l.StatusId == transition.ToStatusId), ct);
        if (membership.HasError)
        {
            return Result.FromError<StatusTransition>(new UnexpectedError());
        }

        var usedStatusIds = membership.Data.Select(l => l.StatusId).ToHashSet();
        if (!usedStatusIds.Contains(transition.FromStatusId) || !usedStatusIds.Contains(transition.ToStatusId))
        {
            return Result.FromError<StatusTransition>(new StatusNotInTypeWorkflowError());
        }

        var insertResult = await transitionRepo.Insert(transition.ToEntityModel(), ct);

        if (insertResult.HasError)
        {
            return insertResult.Error switch
            {
                UniqueConstraintViolationError => await BuildAlreadyExistsError(transition, ct),
                _ => Result.FromError<StatusTransition>(new UnexpectedError()),
            };
        }

        return Result.Ok(insertResult.Data.ToCoreModel());
    }

    public async Task<Result> SoftDeleteTransition(Guid id, CancellationToken ct)
    {
        var result = await transitionRepo.SoftDelete(id, ct);

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

    private async Task<Result<StatusTransition>> BuildAlreadyExistsError(StatusTransition transition, CancellationToken ct)
    {
        var existing = await transitionRepo.GetByExpression(
            t => t.TypeId == transition.TypeId && t.FromStatusId == transition.FromStatusId && t.ToStatusId == transition.ToStatusId, ct);

        return existing.HasError
            ? Result.FromError<StatusTransition>(new UnexpectedError())
            : Result.FromError<StatusTransition>(new StatusTransitionAlreadyExistsError(existing.Data.Id));
    }
}
