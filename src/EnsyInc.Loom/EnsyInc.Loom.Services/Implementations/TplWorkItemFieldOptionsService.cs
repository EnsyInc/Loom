using EnsyInc.Loom.Core.Errors;
using EnsyInc.Loom.Core.Models;
using EnsyInc.Loom.DataAccess.Abstractions;
using EnsyInc.Loom.DataAccess.Mappers;
using EnsyInc.Loom.DataAccess.Models;
using EnsyInc.Loom.Services.Abstractions;

using EnsyNet.Core.Results;
using EnsyNet.DataAccess.Abstractions.Errors;

namespace EnsyInc.Loom.Services.Implementations;

internal sealed class TplWorkItemFieldOptionsService(ITplWorkItemFieldOptionRepo optionRepo, ITplWorkItemFieldRepo fieldRepo) : ITplWorkItemFieldOptionsService
{
    public async Task<Result<IEnumerable<TplWorkItemFieldOption>>> ListOptionsForField(Guid fieldId, CancellationToken ct)
    {
        var result = await optionRepo.GetManyByExpression(o => o.FieldId == fieldId, ct);
        return result.HasError
            ? Result.FromError<IEnumerable<TplWorkItemFieldOption>>(new UnexpectedError())
            : Result.Ok(result.Data.Select(e => e.ToCoreModel()));
    }

    public async Task<Result<TplWorkItemFieldOption>> CreateOption(TplWorkItemFieldOption option, CancellationToken ct)
    {
        var fieldResult = await fieldRepo.GetById(option.FieldId, ct);
        if (fieldResult.HasError)
        {
            return fieldResult.Error switch
            {
                EntityNotFoundError<TplWorkItemFieldEntity> => Result.FromError<TplWorkItemFieldOption>(new TplWorkItemFieldNotFoundError()),
                _ => Result.FromError<TplWorkItemFieldOption>(new UnexpectedError()),
            };
        }

        if (fieldResult.Data.DataType is not (WorkItemFieldDataType.Option or WorkItemFieldDataType.MultiOption))
        {
            return Result.FromError<TplWorkItemFieldOption>(new FieldDoesNotSupportOptionsError());
        }

        var result = await optionRepo.Insert(option.ToEntityModel(), ct);

        if (result.HasError)
        {
            return result.Error switch
            {
                UniqueConstraintViolationError => await BuildValueAlreadyExistsError(option.FieldId, option.Value, ct),
                _ => Result.FromError<TplWorkItemFieldOption>(new UnexpectedError()),
            };
        }

        return Result.Ok(result.Data.ToCoreModel());
    }

    public async Task<Result<TplWorkItemFieldOption>> UpdateOption(TplWorkItemFieldOption option, CancellationToken ct)
    {
        var existingResult = await optionRepo.GetById(option.Id, ct);
        if (existingResult.HasError)
        {
            return existingResult.Error switch
            {
                EntityNotFoundError<TplWorkItemFieldOptionEntity> => Result.FromError<TplWorkItemFieldOption>(new TplWorkItemFieldOptionNotFoundError()),
                _ => Result.FromError<TplWorkItemFieldOption>(new UnexpectedError()),
            };
        }

        var updateResult = await optionRepo.Update(option.Id, updates =>
        {
            updates.AddUpdate(o => o.Label, _ => option.Label);
            updates.AddUpdate(o => o.Rank, _ => option.Rank);
        }, ct);

        if (updateResult.HasError)
        {
            return updateResult.Error switch
            {
                UpdateOperationFailedError => Result.FromError<TplWorkItemFieldOption>(new TplWorkItemFieldOptionNotFoundError()),
                _ => Result.FromError<TplWorkItemFieldOption>(new UnexpectedError()),
            };
        }

        return Result.Ok(existingResult.Data.ToCoreModel() with { Label = option.Label, Rank = option.Rank, UpdatedAt = DateTime.UtcNow });
    }

    public async Task<Result> SoftDeleteOption(Guid id, CancellationToken ct)
    {
        var result = await optionRepo.SoftDelete(id, ct);

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

    private async Task<Result<TplWorkItemFieldOption>> BuildValueAlreadyExistsError(Guid fieldId, string value, CancellationToken ct)
    {
        var existing = await optionRepo.GetByExpression(o => o.FieldId == fieldId && o.Value == value, ct);

        return existing.HasError
            ? Result.FromError<TplWorkItemFieldOption>(new UnexpectedError())
            : Result.FromError<TplWorkItemFieldOption>(new TplWorkItemFieldOptionValueAlreadyExistsError(existing.Data.Id));
    }
}
