using EnsyInc.Loom.Core.Models;

using EnsyNet.Core.Results;

namespace EnsyInc.Loom.Services.Abstractions;

public interface IStatusTransitionsService
{
    public Task<Result<IEnumerable<StatusTransition>>> ListTransitionsForType(Guid typeId, CancellationToken ct);

    /// <summary>
    /// Creates an allowed move between two statuses for a work item type's
    /// workflow. Both statuses must already be ones the type uses (see
    /// <see cref="IWorkItemTypeStatusesService.AddStatusToType"/>).
    /// </summary>
    public Task<Result<StatusTransition>> CreateTransition(StatusTransition transition, CancellationToken ct);

    public Task<Result> SoftDeleteTransition(Guid id, CancellationToken ct);
}
