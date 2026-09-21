using EnsyInc.Loom.Core.Errors;
using EnsyInc.Loom.Core.Models;
using EnsyInc.Loom.DataAccess.Abstractions;
using EnsyInc.Loom.DataAccess.Mappers;
using EnsyInc.Loom.DataAccess.Models;
using EnsyInc.Loom.Services.Abstractions;

using EnsyNet.Core.Results;
using EnsyNet.DataAccess.Abstractions.Errors;

namespace EnsyInc.Loom.Services.Implementations;

internal sealed class ProjectsService(IProjectRepo projectRepo) : IProjectsService
{
    public async Task<Result<IEnumerable<Project>>> ListProjects(CancellationToken ct)
    {
        var result = await projectRepo.GetAll(ct);
        return result.HasError
            ? Result.FromError<IEnumerable<Project>>(new UnexpectedError())
            : Result.Ok(result.Data.Select(e => e.ToCoreModel()));
    }

    public async Task<Result<Project>> GetProject(Guid id, CancellationToken ct)
    {
        var result = await projectRepo.GetById(id, ct);

        if (result.HasError)
        {
            return result.Error switch
            {
                EntityNotFoundError<ProjectEntity> => Result.FromError<Project>(new ProjectNotFoundError()),
                _ => Result.FromError<Project>(new UnexpectedError()),
            };
        }

        return Result.Ok(result.Data.ToCoreModel());
    }

    public async Task<Result<Project>> CreateProject(Project project, CancellationToken ct)
    {
        var result = await projectRepo.Insert(project.ToEntityModel(), ct);
        return result.HasError
            ? Result.FromError<Project>(new UnexpectedError())
            : Result.Ok(result.Data.ToCoreModel());
    }

    public async Task<Result<Project>> UpdateProject(Project project, CancellationToken ct)
    {
        var existing = await GetProject(project.Id, ct);
        if (existing.HasError)
        {
            return Result.FromError<Project>(existing.Error);
        }

        var updateResult = await projectRepo.Update(project.Id, updates => updates.AddUpdate(p => p.Name, _ => project.Name), ct);

        if (updateResult.HasError)
        {
            return updateResult.Error switch
            {
                UpdateOperationFailedError => Result.FromError<Project>(new ProjectNotFoundError()),
                _ => Result.FromError<Project>(new UnexpectedError()),
            };
        }

        return Result.Ok(existing.Data with { Name = project.Name, UpdatedAt = DateTime.UtcNow });
    }

    public async Task<Result> SoftDeleteProject(Guid id, CancellationToken ct)
    {
        var result = await projectRepo.SoftDelete(id, ct);

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
}
