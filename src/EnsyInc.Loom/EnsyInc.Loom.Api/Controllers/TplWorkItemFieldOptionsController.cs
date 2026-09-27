using EnsyInc.Loom.Api.Exceptions;
using EnsyInc.Loom.Api.Models;
using EnsyInc.Loom.Api.Models.Mappers;
using EnsyInc.Loom.Core.Errors;
using EnsyInc.Loom.Services.Abstractions;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;

namespace EnsyInc.Loom.Api.Controllers;

/// <summary>Manage a field's options.</summary>
[ApiController]
[Route("fields/{fieldId:guid}/options")]
[Produces("application/json")]
public sealed class TplWorkItemFieldOptionsController(
    ITplWorkItemFieldOptionsService optionsService,
    IValidator<CreateFieldOptionRequest> createOptionValidator,
    IValidator<UpdateFieldOptionRequest> updateOptionValidator)
    : ControllerBase
{
    /// <summary>Lists a field's options.</summary>
    /// <param name="fieldId">The field's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The field's options.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetFieldOptionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOptions(Guid fieldId, CancellationToken ct)
    {
        var result = await optionsService.ListOptionsForField(fieldId, ct);

        return result switch
        {
            { HasError: false } => Ok(new GetFieldOptionsResponse(result.Data.Select(x => x.ToPublicModel()))),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Creates a new option on a field. The field must be an Option or MultiOption field.</summary>
    /// <param name="fieldId">The field's id.</param>
    /// <param name="request">The option to create.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">The option was created.</response>
    /// <response code="400">The request failed validation, or the field is not an Option or MultiOption field.</response>
    /// <response code="404">No field exists with the given id.</response>
    /// <response code="409">An option with this value already exists on the field.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost]
    [ProducesResponseType(typeof(GetFieldOptionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOption(Guid fieldId, CreateFieldOptionRequest request, CancellationToken ct)
    {
        await createOptionValidator.ValidateAndThrowAsync(request, ct);

        var option = request.ToCoreModel(fieldId);
        var result = await optionsService.CreateOption(option, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data.ToPublicModel()),
            { HasError: true, Error: TplWorkItemFieldNotFoundError } => NotFound(ErrorResponses.TplWorkItemFieldNotFoundError),
            { HasError: true, Error: FieldDoesNotSupportOptionsError } => BadRequest(ErrorResponses.FieldDoesNotSupportOptionsError),
            { HasError: true, Error: TplWorkItemFieldOptionValueAlreadyExistsError e } => Conflict(ErrorResponses.TplWorkItemFieldOptionValueAlreadyExistsError(e.ExistingOptionId)),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Updates an existing option's label and rank. Its value is immutable after creation.</summary>
    /// <param name="id">The option's id.</param>
    /// <param name="request">The new option values.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The updated option.</response>
    /// <response code="400">The request failed validation (e.g. a missing label).</response>
    /// <response code="404">No option exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(GetFieldOptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateOption(Guid id, UpdateFieldOptionRequest request, CancellationToken ct)
    {
        await updateOptionValidator.ValidateAndThrowAsync(request, ct);

        var result = await optionsService.UpdateOption(id, request.Label, request.Rank, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data.ToPublicModel()),
            { HasError: true, Error: TplWorkItemFieldOptionNotFoundError } => NotFound(ErrorResponses.TplWorkItemFieldOptionNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Deletes an option. Idempotent: deleting an option that doesn't exist (or was already deleted) still succeeds.</summary>
    /// <param name="id">The option's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">The option is deleted (or was already gone).</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteOption(Guid id, CancellationToken ct)
    {
        var result = await optionsService.SoftDeleteOption(id, ct);

        return result switch
        {
            { HasError: false } => NoContent(),
            _ => throw new UnhandledResultErrorException(),
        };
    }
}
