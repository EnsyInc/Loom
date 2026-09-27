using EnsyInc.Loom.Core.Models;

using EnsyNet.Core.Results;

namespace EnsyInc.Loom.Services.Abstractions;

public interface ITplWorkItemFieldsService
{
    public Task<Result<IEnumerable<TplWorkItemField>>> ListFieldsForType(Guid typeId, CancellationToken ct);

    public Task<Result<TplWorkItemField>> GetField(Guid id, CancellationToken ct);

    public Task<Result<TplWorkItemField>> CreateField(TplWorkItemField field, CancellationToken ct);

    /// <summary>
    /// Updates the field's label, required flag, and default value. Its key
    /// and data type are immutable after creation.
    /// </summary>
    public Task<Result<TplWorkItemField>> UpdateField(TplWorkItemField field, CancellationToken ct);

    public Task<Result> SoftDeleteField(Guid id, CancellationToken ct);
}
