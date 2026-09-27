using EnsyInc.Loom.Core.Errors;
using EnsyInc.Loom.Core.Models;
using EnsyInc.Loom.DataAccess.Abstractions;
using EnsyInc.Loom.DataAccess.Mappers;
using EnsyInc.Loom.DataAccess.Models;
using EnsyInc.Loom.Services.Abstractions;

using EnsyNet.Core.Results;
using EnsyNet.DataAccess.Abstractions.Errors;

namespace EnsyInc.Loom.Services.Implementations;

internal sealed class ProjectWorkItemTypesService(
    IProjectWorkItemTypeRepo projectTypeRepo,
    IProjectRepo projectRepo,
    ITplWorkItemRepo typeRepo) : IProjectWorkItemTypesService
{
    public async Task<Result<IEnumerable<TplWorkItem>>> ListTypesForProject(Guid projectId, CancellationToken ct)
    {
        var links = await projectTypeRepo.GetManyByExpression(l => l.ProjectId == projectId, ct);
        if (links.HasError)
        {
            return Result.FromError<IEnumerable<TplWorkItem>>(new UnexpectedError());
        }

        var typeIds = links.Data.Select(l => l.TypeId).ToHashSet();
        var types = await typeRepo.GetManyByExpression(t => typeIds.Contains(t.Id), ct);

        return types.HasError
            ? Result.FromError<IEnumerable<TplWorkItem>>(new UnexpectedError())
            : Result.Ok(types.Data.Select(e => e.ToCoreModel()));
    }

    public async Task<Result<IEnumerable<Project>>> ListProjectsForType(Guid typeId, CancellationToken ct)
    {
        var links = await projectTypeRepo.GetManyByExpression(l => l.TypeId == typeId, ct);
        if (links.HasError)
        {
            return Result.FromError<IEnumerable<Project>>(new UnexpectedError());
        }

        var projectIds = links.Data.Select(l => l.ProjectId).ToHashSet();
        var projects = await projectRepo.GetManyByExpression(p => projectIds.Contains(p.Id), ct);

        return projects.HasError
            ? Result.FromError<IEnumerable<Project>>(new UnexpectedError())
            : Result.Ok(projects.Data.Select(e => e.ToCoreModel()));
    }

    public async Task<Result> OptIn(Guid projectId, Guid typeId, CancellationToken ct)
    {
        var projectResult = await projectRepo.GetById(projectId, ct);
        if (projectResult.HasError)
        {
            return projectResult.Error switch
            {
                EntityNotFoundError<ProjectEntity> => Result.FromError(new ProjectNotFoundError()),
                _ => Result.FromError(new UnexpectedError()),
            };
        }

        var typeResult = await typeRepo.GetById(typeId, ct);
        if (typeResult.HasError)
        {
            return typeResult.Error switch
            {
                EntityNotFoundError<TplWorkItemEntity> => Result.FromError(new TplWorkItemNotFoundError()),
                _ => Result.FromError(new UnexpectedError()),
            };
        }

        var existingLink = await projectTypeRepo.GetManyByExpression(l => l.ProjectId == projectId && l.TypeId == typeId, ct);
        if (existingLink.HasError)
        {
            return Result.FromError(new UnexpectedError());
        }

        if (existingLink.Data.Any())
        {
            return Result.Ok();
        }

        var link = new ProjectWorkItemType { ProjectId = projectId, TypeId = typeId };
        var insertResult = await projectTypeRepo.Insert(link.ToEntityModel(), ct);

        if (insertResult.HasError)
        {
            return insertResult.Error switch
            {
                UniqueConstraintViolationError => Result.Ok(),
                _ => Result.FromError(new UnexpectedError()),
            };
        }

        return Result.Ok();
    }

    // TODO: this should block the opt-out while a non-deleted WorkItem in the project still uses the type.
    public async Task<Result> OptOut(Guid projectId, Guid typeId, CancellationToken ct)
    {
        var existingLink = await projectTypeRepo.GetManyByExpression(l => l.ProjectId == projectId && l.TypeId == typeId, ct);
        if (existingLink.HasError)
        {
            return Result.FromError(new UnexpectedError());
        }

        var link = existingLink.Data.FirstOrDefault();
        if (link is null)
        {
            return Result.Ok();
        }

        var deleteResult = await projectTypeRepo.SoftDelete(link.Id, ct);

        if (deleteResult.HasError)
        {
            return deleteResult.Error switch
            {
                DeleteOperationFailedError => Result.Ok(),
                _ => Result.FromError(new UnexpectedError()),
            };
        }

        return Result.Ok();
    }
}
