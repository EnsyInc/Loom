using EnsyInc.Loom.Core.Models;

using EnsyNet.Core.Results;

namespace EnsyInc.Loom.Services.Abstractions;

public interface IProjectWorkItemTypesService
{
    public Task<Result<IEnumerable<TplWorkItem>>> ListTypesForProject(Guid projectId, CancellationToken ct);

    public Task<Result<IEnumerable<Project>>> ListProjectsForType(Guid typeId, CancellationToken ct);

    /// <summary>
    /// Opts a project into a work item type. Idempotent — opting into a type
    /// the project already uses is a no-op.
    /// </summary>
    public Task<Result> OptIn(Guid projectId, Guid typeId, CancellationToken ct);

    /// <summary>
    /// Opts a project out of a work item type. Idempotent — opting out of a
    /// type the project doesn't use is a no-op.
    /// </summary>
    public Task<Result> OptOut(Guid projectId, Guid typeId, CancellationToken ct);
}
