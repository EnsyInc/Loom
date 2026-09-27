using EnsyInc.Loom.Core.Models;

using EnsyNet.Core.Results;

namespace EnsyInc.Loom.Services.Abstractions;

public interface ITplWorkItemFieldOptionsService
{
    public Task<Result<IEnumerable<TplWorkItemFieldOption>>> ListOptionsForField(Guid fieldId, CancellationToken ct);

    /// <summary>
    /// Creates an option for a field. The field must be an
    /// <see cref="WorkItemFieldDataType.Option"/> or
    /// <see cref="WorkItemFieldDataType.MultiOption"/> field.
    /// </summary>
    public Task<Result<TplWorkItemFieldOption>> CreateOption(TplWorkItemFieldOption option, CancellationToken ct);

    /// <summary>
    /// Updates the option's label and rank. Its value is immutable after
    /// creation.
    /// </summary>
    public Task<Result<TplWorkItemFieldOption>> UpdateOption(Guid id, string label, int rank, CancellationToken ct);

    public Task<Result> SoftDeleteOption(Guid id, CancellationToken ct);
}
