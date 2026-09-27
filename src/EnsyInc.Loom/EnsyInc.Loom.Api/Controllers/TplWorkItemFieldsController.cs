using EnsyInc.Loom.Api.Exceptions;
using EnsyInc.Loom.Api.Models;
using EnsyInc.Loom.Api.Models.Mappers;
using EnsyInc.Loom.Core.Errors;
using EnsyInc.Loom.Services.Abstractions;

using FluentValidation;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnsyInc.Loom.Api.Controllers;

/// <summary>Manage a work item type's custom fields.</summary>
[ApiController]
[Authorize]
[Route("work-item-types/{typeId:guid}/fields")]
[Produces("application/json")]
public sealed class TplWorkItemFieldsController(
    ITplWorkItemFieldsService fieldsService,
    IValidator<CreateWorkItemFieldRequest> createFieldValidator,
    IValidator<UpdateWorkItemFieldRequest> updateFieldValidator)
    : ControllerBase
{
    /// <summary>Lists a work item type's fields.</summary>
    /// <param name="typeId">The type's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The type's fields.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetWorkItemFieldsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFields(Guid typeId, CancellationToken ct)
    {
        var result = await fieldsService.ListFieldsForType(typeId, ct);

        return result switch
        {
            { HasError: false } => Ok(new GetWorkItemFieldsResponse(result.Data.Select(x => x.ToPublicModel()))),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Gets a single field by id.</summary>
    /// <param name="id">The field's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The field.</response>
    /// <response code="404">No field exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetWorkItemFieldResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetField(Guid id, CancellationToken ct)
    {
        var result = await fieldsService.GetField(id, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data.ToPublicModel()),
            { HasError: true, Error: TplWorkItemFieldNotFoundError } => NotFound(ErrorResponses.TplWorkItemFieldNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Creates a new field on a work item type.</summary>
    /// <param name="typeId">The type's id.</param>
    /// <param name="request">The field to create.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">The field was created. The response body and <c>Location</c> header describe the new field.</response>
    /// <response code="400">The request failed validation (e.g. a missing key).</response>
    /// <response code="404">No work item type exists with the given id.</response>
    /// <response code="409">A field with this key already exists on the type.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost]
    [ProducesResponseType(typeof(GetWorkItemFieldResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateField(Guid typeId, CreateWorkItemFieldRequest request, CancellationToken ct)
    {
        await createFieldValidator.ValidateAndThrowAsync(request, ct);

        var field = request.ToCoreModel(typeId);
        var result = await fieldsService.CreateField(field, ct);

        return result switch
        {
            { HasError: false } => CreatedAtAction(nameof(GetField), new { typeId, id = result.Data.Id }, result.Data.ToPublicModel()),
            { HasError: true, Error: TplWorkItemNotFoundError } => NotFound(ErrorResponses.TplWorkItemNotFoundError),
            { HasError: true, Error: TplWorkItemFieldKeyAlreadyExistsError e } => Conflict(ErrorResponses.TplWorkItemFieldKeyAlreadyExistsError(e.ExistingFieldId)),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Updates an existing field's label, required flag, and default value. Its key and data type are immutable after creation.</summary>
    /// <param name="id">The field's id.</param>
    /// <param name="request">The new field values.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The updated field.</response>
    /// <response code="400">The request failed validation (e.g. a missing label).</response>
    /// <response code="404">No field exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(GetWorkItemFieldResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateField(Guid id, UpdateWorkItemFieldRequest request, CancellationToken ct)
    {
        await updateFieldValidator.ValidateAndThrowAsync(request, ct);

        var result = await fieldsService.UpdateField(id, request.Label, request.Required, request.DefaultValue, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data.ToPublicModel()),
            { HasError: true, Error: TplWorkItemFieldNotFoundError } => NotFound(ErrorResponses.TplWorkItemFieldNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Deletes a field. Idempotent: deleting a field that doesn't exist (or was already deleted) still succeeds.</summary>
    /// <param name="id">The field's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">The field is deleted (or was already gone).</response>
    /// <response code="409">The field still has options.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteField(Guid id, CancellationToken ct)
    {
        var result = await fieldsService.SoftDeleteField(id, ct);

        return result switch
        {
            { HasError: false } => NoContent(),
            { HasError: true, Error: TplWorkItemFieldInUseError } => Conflict(ErrorResponses.TplWorkItemFieldInUseError),
            _ => throw new UnhandledResultErrorException(),
        };
    }
}
