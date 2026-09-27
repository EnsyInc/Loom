using EnsyInc.Loom.Core.Models;

using EnsyNet.Core.Results;

namespace EnsyInc.Loom.Services.Abstractions;

public interface ITplWorkItemsService
{
    public Task<Result<IEnumerable<TplWorkItem>>> ListTypes(CancellationToken ct);

    public Task<Result<TplWorkItem>> GetType(Guid id, CancellationToken ct);

    /// <summary>
    /// Creates a work item type. The type has no initial status yet — use
    /// <see cref="SetInitialStatus"/> once it has at least one status via
    /// <see cref="IWorkItemTypeStatusesService.AddStatusToType"/>.
    /// </summary>
    public Task<Result<TplWorkItem>> CreateType(TplWorkItem type, CancellationToken ct);

    /// <summary>
    /// Updates the type's name and icon. Its initial status is unaffected —
    /// use <see cref="SetInitialStatus"/> for that.
    /// </summary>
    public Task<Result<TplWorkItem>> UpdateType(TplWorkItem type, CancellationToken ct);

    /// <summary>
    /// Sets the status new items of this type start in. The status must
    /// already be one of the type's statuses (see
    /// <see cref="IWorkItemTypeStatusesService.AddStatusToType"/>).
    /// </summary>
    public Task<Result<TplWorkItem>> SetInitialStatus(Guid typeId, Guid statusId, CancellationToken ct);

    public Task<Result> SoftDeleteType(Guid id, CancellationToken ct);
}
