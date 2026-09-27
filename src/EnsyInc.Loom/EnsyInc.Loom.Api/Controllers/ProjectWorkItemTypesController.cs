using EnsyInc.Loom.Api.Exceptions;
using EnsyInc.Loom.Api.Models;
using EnsyInc.Loom.Api.Models.Mappers;
using EnsyInc.Loom.Core.Errors;
using EnsyInc.Loom.Services.Abstractions;

using Microsoft.AspNetCore.Mvc;

namespace EnsyInc.Loom.Api.Controllers;

/// <summary>Manage which work item types a project has opted into.</summary>
[ApiController]
[Route("projects/{projectId:guid}/work-item-types")]
[Produces("application/json")]
public sealed class ProjectWorkItemTypesController(IProjectWorkItemTypesService projectTypesService) : ControllerBase
{
    /// <summary>Lists the work item types a project has opted into.</summary>
    /// <param name="projectId">The project's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The project's work item types.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetProjectWorkItemTypesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTypesForProject(Guid projectId, CancellationToken ct)
    {
        var result = await projectTypesService.ListTypesForProject(projectId, ct);

        return result switch
        {
            { HasError: false } => Ok(new GetProjectWorkItemTypesResponse(result.Data.Select(x => x.ToPublicModel()))),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Lists the projects that have opted into a work item type.</summary>
    /// <param name="typeId">The type's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The type's projects.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet("/work-item-types/{typeId:guid}/projects")]
    [ProducesResponseType(typeof(GetWorkItemTypeProjectsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProjectsForType(Guid typeId, CancellationToken ct)
    {
        var result = await projectTypesService.ListProjectsForType(typeId, ct);

        return result switch
        {
            { HasError: false } => Ok(new GetWorkItemTypeProjectsResponse(result.Data.Select(x => x.ToPublicModel()))),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Opts a project into a work item type. Idempotent: opting into a type the project already uses still succeeds.</summary>
    /// <param name="projectId">The project's id.</param>
    /// <param name="typeId">The type's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">The project now uses the type.</response>
    /// <response code="404">No project or work item type exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPut("{typeId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> OptIn(Guid projectId, Guid typeId, CancellationToken ct)
    {
        var result = await projectTypesService.OptIn(projectId, typeId, ct);

        return result switch
        {
            { HasError: false } => NoContent(),
            { HasError: true, Error: ProjectNotFoundError } => NotFound(ErrorResponses.ProjectNotFoundError),
            { HasError: true, Error: TplWorkItemNotFoundError } => NotFound(ErrorResponses.TplWorkItemNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Opts a project out of a work item type. Idempotent: opting out of a type the project doesn't use still succeeds.</summary>
    /// <param name="projectId">The project's id.</param>
    /// <param name="typeId">The type's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">The project no longer uses the type (or already didn't).</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpDelete("{typeId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> OptOut(Guid projectId, Guid typeId, CancellationToken ct)
    {
        var result = await projectTypesService.OptOut(projectId, typeId, ct);

        return result switch
        {
            { HasError: false } => NoContent(),
            _ => throw new UnhandledResultErrorException(),
        };
    }
}
