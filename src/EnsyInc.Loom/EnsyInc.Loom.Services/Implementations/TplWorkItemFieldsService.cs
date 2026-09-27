using EnsyInc.Loom.Core.Errors;
using EnsyInc.Loom.Core.Models;
using EnsyInc.Loom.DataAccess.Abstractions;
using EnsyInc.Loom.DataAccess.Mappers;
using EnsyInc.Loom.DataAccess.Models;
using EnsyInc.Loom.Services.Abstractions;

using EnsyNet.Core.Results;
using EnsyNet.DataAccess.Abstractions.Errors;

namespace EnsyInc.Loom.Services.Implementations;

internal sealed class TplWorkItemFieldsService(
    ITplWorkItemFieldRepo fieldRepo,
    ITplWorkItemRepo typeRepo,
    ITplWorkItemFieldOptionRepo fieldOptionRepo) : ITplWorkItemFieldsService
{
    public async Task<Result<IEnumerable<TplWorkItemField>>> ListFieldsForType(Guid typeId, CancellationToken ct)
    {
        var result = await fieldRepo.GetManyByExpression(f => f.TypeId == typeId, ct);
        return result.HasError
            ? Result.FromError<IEnumerable<TplWorkItemField>>(new UnexpectedError())
            : Result.Ok(result.Data.Select(e => e.ToCoreModel()));
    }

    public async Task<Result<TplWorkItemField>> GetField(Guid id, CancellationToken ct)
    {
        var result = await fieldRepo.GetById(id, ct);

        if (result.HasError)
        {
            return result.Error switch
            {
                EntityNotFoundError<TplWorkItemFieldEntity> => Result.FromError<TplWorkItemField>(new TplWorkItemFieldNotFoundError()),
                _ => Result.FromError<TplWorkItemField>(new UnexpectedError()),
            };
        }

        return Result.Ok(result.Data.ToCoreModel());
    }

    public async Task<Result<TplWorkItemField>> CreateField(TplWorkItemField field, CancellationToken ct)
    {
        var typeResult = await typeRepo.GetById(field.TypeId, ct);
        if (typeResult.HasError)
        {
            return typeResult.Error switch
            {
                EntityNotFoundError<TplWorkItemEntity> => Result.FromError<TplWorkItemField>(new TplWorkItemNotFoundError()),
                _ => Result.FromError<TplWorkItemField>(new UnexpectedError()),
            };
        }

        var result = await fieldRepo.Insert(field.ToEntityModel(), ct);

        if (result.HasError)
        {
            return result.Error switch
            {
                UniqueConstraintViolationError => await BuildKeyAlreadyExistsError(field.TypeId, field.Key, ct),
                _ => Result.FromError<TplWorkItemField>(new UnexpectedError()),
            };
        }

        return Result.Ok(result.Data.ToCoreModel());
    }

    public async Task<Result<TplWorkItemField>> UpdateField(Guid id, string label, bool required, string? defaultValue, CancellationToken ct)
    {
        var existing = await GetField(id, ct);
        if (existing.HasError)
        {
            return Result.FromError<TplWorkItemField>(existing.Error);
        }

        var updateResult = await fieldRepo.Update(id, updates =>
        {
            updates.AddUpdate(f => f.Label, _ => label);
            updates.AddUpdate(f => f.Required, _ => required);
            updates.AddUpdate(f => f.DefaultValue, _ => defaultValue);
        }, ct);

        if (updateResult.HasError)
        {
            return updateResult.Error switch
            {
                UpdateOperationFailedError => Result.FromError<TplWorkItemField>(new TplWorkItemFieldNotFoundError()),
                _ => Result.FromError<TplWorkItemField>(new UnexpectedError()),
            };
        }

        return Result.Ok(existing.Data with { Label = label, Required = required, DefaultValue = defaultValue, UpdatedAt = DateTime.UtcNow });
    }

    public async Task<Result> SoftDeleteField(Guid id, CancellationToken ct)
    {
        var usages = await fieldOptionRepo.GetManyByExpression(o => o.FieldId == id, ct);
        if (usages.HasError)
        {
            return Result.FromError(new UnexpectedError());
        }

        if (usages.Data.Any())
        {
            return Result.FromError(new TplWorkItemFieldInUseError());
        }

        var result = await fieldRepo.SoftDelete(id, ct);

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

    private async Task<Result<TplWorkItemField>> BuildKeyAlreadyExistsError(Guid typeId, string key, CancellationToken ct)
    {
        var existing = await fieldRepo.GetByExpression(f => f.TypeId == typeId && f.Key == key, ct);

        return existing.HasError
            ? Result.FromError<TplWorkItemField>(new UnexpectedError())
            : Result.FromError<TplWorkItemField>(new TplWorkItemFieldKeyAlreadyExistsError(existing.Data.Id));
    }
}
