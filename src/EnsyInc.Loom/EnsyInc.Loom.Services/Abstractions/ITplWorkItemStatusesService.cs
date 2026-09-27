using EnsyInc.Loom.Core.Models;

using EnsyNet.Core.Results;

namespace EnsyInc.Loom.Services.Abstractions;

public interface ITplWorkItemStatusesService
{
    public Task<Result<IEnumerable<TplWorkItemStatus>>> ListStatuses(CancellationToken ct);

    public Task<Result<TplWorkItemStatus>> GetStatus(Guid id, CancellationToken ct);

    public Task<Result<TplWorkItemStatus>> CreateStatus(TplWorkItemStatus status, CancellationToken ct);

    public Task<Result<TplWorkItemStatus>> UpdateStatus(TplWorkItemStatus status, CancellationToken ct);

    public Task<Result> SoftDeleteStatus(Guid id, CancellationToken ct);
}
