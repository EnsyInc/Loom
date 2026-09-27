using EnsyInc.Loom.Api.Exceptions;
using EnsyInc.Loom.Api.Models;
using EnsyInc.Loom.Api.Models.Mappers;
using EnsyInc.Loom.Core.Errors;
using EnsyInc.Loom.Services.Abstractions;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;

namespace EnsyInc.Loom.Api.Controllers;

/// <summary>Manage work item types, the shared definitions projects opt into.</summary>
[ApiController]
[Route("work-item-types")]
[Produces("application/json")]
public sealed class TplWorkItemTypesController(
    ITplWorkItemsService typesService,
    IValidator<CreateWorkItemTypeRequest> createTypeValidator,
    IValidator<UpdateWorkItemTypeRequest> updateTypeValidator,
    IValidator<SetInitialStatusRequest> setInitialStatusValidator)
    : ControllerBase
{
    /// <summary>Lists all work item types.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The work item types.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetWorkItemTypesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWorkItemTypes(CancellationToken ct)
    {
        var result = await typesService.ListTypes(ct);

        return result switch
        {
            { HasError: false } => Ok(new GetWorkItemTypesResponse(result.Data.Select(x => x.ToPublicModel()))),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Gets a single work item type by id.</summary>
    /// <param name="id">The type's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The work item type.</response>
    /// <response code="404">No work item type exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetWorkItemTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWorkItemType(Guid id, CancellationToken ct)
    {
        var result = await typesService.GetType(id, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data.ToPublicModel()),
            { HasError: true, Error: TplWorkItemNotFoundError } => NotFound(ErrorResponses.TplWorkItemNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Creates a new work item type. It has no initial status yet.</summary>
    /// <param name="request">The type to create.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">The type was created. The response body and <c>Location</c> header describe the new type.</response>
    /// <response code="400">The request failed validation (e.g. a missing name).</response>
    /// <response code="409">A work item type with this name already exists.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost]
    [ProducesResponseType(typeof(GetWorkItemTypeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateWorkItemType(CreateWorkItemTypeRequest request, CancellationToken ct)
    {
        await createTypeValidator.ValidateAndThrowAsync(request, ct);

        var type = request.ToCoreModel();
        var result = await typesService.CreateType(type, ct);

        return result switch
        {
            { HasError: false } => CreatedAtAction(nameof(GetWorkItemType), new { id = result.Data.Id }, result.Data.ToPublicModel()),
            { HasError: true, Error: TplWorkItemNameAlreadyExistsError e } => Conflict(ErrorResponses.TplWorkItemNameAlreadyExistsError(e.ExistingTypeId)),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Updates an existing work item type's name and icon.</summary>
    /// <param name="id">The type's id.</param>
    /// <param name="request">The new field values.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The updated work item type.</response>
    /// <response code="400">The request failed validation (e.g. a missing name).</response>
    /// <response code="404">No work item type exists with the given id.</response>
    /// <response code="409">A work item type with this name already exists.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(GetWorkItemTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateWorkItemType(Guid id, UpdateWorkItemTypeRequest request, CancellationToken ct)
    {
        await updateTypeValidator.ValidateAndThrowAsync(request, ct);

        var type = request.ToCoreModel(id);
        var result = await typesService.UpdateType(type, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data.ToPublicModel()),
            { HasError: true, Error: TplWorkItemNotFoundError } => NotFound(ErrorResponses.TplWorkItemNotFoundError),
            { HasError: true, Error: TplWorkItemNameAlreadyExistsError e } => Conflict(ErrorResponses.TplWorkItemNameAlreadyExistsError(e.ExistingTypeId)),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Sets the status new items of this type start in. The status must already be one of the type's statuses.</summary>
    /// <param name="id">The type's id.</param>
    /// <param name="request">The status to set as initial.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The updated work item type.</response>
    /// <response code="400">The request failed validation, or the status is not one of the type's statuses.</response>
    /// <response code="404">No work item type exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPut("{id:guid}/initial-status")]
    [ProducesResponseType(typeof(GetWorkItemTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetInitialStatus(Guid id, SetInitialStatusRequest request, CancellationToken ct)
    {
        await setInitialStatusValidator.ValidateAndThrowAsync(request, ct);

        var result = await typesService.SetInitialStatus(id, request.StatusId, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data.ToPublicModel()),
            { HasError: true, Error: TplWorkItemNotFoundError } => NotFound(ErrorResponses.TplWorkItemNotFoundError),
            { HasError: true, Error: StatusNotInTypeWorkflowError } => BadRequest(ErrorResponses.StatusNotInTypeWorkflowError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Deletes a work item type. Idempotent: deleting a type that doesn't exist (or was already deleted) still succeeds.</summary>
    /// <param name="id">The type's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">The type is deleted (or was already gone).</response>
    /// <response code="409">The type is used by at least one project.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteWorkItemType(Guid id, CancellationToken ct)
    {
        var result = await typesService.SoftDeleteType(id, ct);

        return result switch
        {
            { HasError: false } => NoContent(),
            { HasError: true, Error: TplWorkItemInUseError } => Conflict(ErrorResponses.TplWorkItemInUseError),
            _ => throw new UnhandledResultErrorException(),
        };
    }
}
