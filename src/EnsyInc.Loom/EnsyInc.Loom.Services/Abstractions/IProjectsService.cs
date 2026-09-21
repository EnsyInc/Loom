using EnsyInc.Loom.Core.Models;

using EnsyNet.Core.Results;

namespace EnsyInc.Loom.Services.Abstractions;

public interface IProjectsService
{
    public Task<Result<IEnumerable<Project>>> ListProjects(CancellationToken ct);
    public Task<Result<Project>> GetProject(Guid id, CancellationToken ct);

    public Task<Result<Project>> CreateProject(Project project, CancellationToken ct);

    public Task<Result<Project>> UpdateProject(Project project, CancellationToken ct);

    public Task<Result> SoftDeleteProject(Guid id, CancellationToken ct);
}
