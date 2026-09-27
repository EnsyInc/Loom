using EnsyInc.Loom.Core.Models;

using EnsyNet.Core.Results;

namespace EnsyInc.Loom.Services.Abstractions;

public interface IWorkItemTypeStatusesService
{
    public Task<Result<IEnumerable<TplWorkItemStatus>>> ListStatusesForType(Guid typeId, CancellationToken ct);

    /// <summary>
    /// Opts a work item type into a status. Idempotent — adding a status the
    /// type already uses is a no-op.
    /// </summary>
    public Task<Result> AddStatusToType(Guid typeId, Guid statusId, CancellationToken ct);

    /// <summary>
    /// Removes a status from a work item type. Idempotent — removing a
    /// status the type doesn't use is a no-op. Fails if the status is the
    /// type's initial status, or if any <see cref="StatusTransition"/> for
    /// the type still references it.
    /// </summary>
    public Task<Result> RemoveStatusFromType(Guid typeId, Guid statusId, CancellationToken ct);
}
