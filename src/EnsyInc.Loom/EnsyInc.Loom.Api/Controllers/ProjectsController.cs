using EnsyInc.Loom.Api.Exceptions;
using EnsyInc.Loom.Api.Models;
using EnsyInc.Loom.Api.Models.Mappers;
using EnsyInc.Loom.Core.Errors;
using EnsyInc.Loom.Services.Abstractions;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;

namespace EnsyInc.Loom.Api.Controllers;

/// <summary>Manage projects, the containers for work items.</summary>
[ApiController]
[Route("projects")]
[Produces("application/json")]
public sealed class ProjectsController(
    IProjectsService projectsService,
    IValidator<CreateProjectRequest> createProjectValidator,
    IValidator<UpdateProjectRequest> updateProjectValidator)
    : ControllerBase
{
    /// <summary>Lists all projects.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The projects.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetProjectsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProjects(CancellationToken ct)
    {
        var result = await projectsService.ListProjects(ct);

        return result switch
        {
            { HasError: false } => Ok(new GetProjectsResponse(result.Data.Select(x => x.ToPublicModel()))),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Gets a single project by id.</summary>
    /// <param name="id">The project's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The project.</response>
    /// <response code="404">No project exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetProjectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProject(Guid id, CancellationToken ct)
    {
        var result = await projectsService.GetProject(id, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data.ToPublicModel()),
            { HasError: true, Error: ProjectNotFoundError } => NotFound(ErrorResponses.ProjectNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Creates a new project.</summary>
    /// <param name="request">The project to create.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">The project was created. The response body and <c>Location</c> header describe the new project.</response>
    /// <response code="400">The request failed validation (e.g. a missing name).</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost]
    [ProducesResponseType(typeof(GetProjectResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateProject(CreateProjectRequest request, CancellationToken ct)
    {
        await createProjectValidator.ValidateAndThrowAsync(request, ct);

        var project = request.ToCoreModel();
        var result = await projectsService.CreateProject(project, ct);

        return result switch
        {
            { HasError: false } => CreatedAtAction(nameof(GetProject), new { id = result.Data.Id }, result.Data.ToPublicModel()),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Updates an existing project.</summary>
    /// <param name="id">The project's id.</param>
    /// <param name="request">The new field values.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The updated project.</response>
    /// <response code="400">The request failed validation (e.g. a missing name).</response>
    /// <response code="404">No project exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(GetProjectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProject(Guid id, UpdateProjectRequest request, CancellationToken ct)
    {
        await updateProjectValidator.ValidateAndThrowAsync(request, ct);

        var project = request.ToCoreModel(id);
        var result = await projectsService.UpdateProject(project, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data.ToPublicModel()),
            { HasError: true, Error: ProjectNotFoundError } => NotFound(ErrorResponses.ProjectNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Deletes a project.</summary>
    /// <param name="id">The project's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">The project is deleted (or was already gone).</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProject(Guid id, CancellationToken ct)
    {
        var result = await projectsService.SoftDeleteProject(id, ct);

        return result switch
        {
            { HasError: false } => NoContent(),
            _ => throw new UnhandledResultErrorException(),
        };
    }
}
