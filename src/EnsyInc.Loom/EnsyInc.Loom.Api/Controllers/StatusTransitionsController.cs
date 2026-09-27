using EnsyInc.Loom.Api.Exceptions;
using EnsyInc.Loom.Api.Models;
using EnsyInc.Loom.Api.Models.Mappers;
using EnsyInc.Loom.Core.Errors;
using EnsyInc.Loom.Services.Abstractions;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;

namespace EnsyInc.Loom.Api.Controllers;

/// <summary>Manage a work item type's allowed status transitions.</summary>
[ApiController]
[Route("work-item-types/{typeId:guid}/transitions")]
[Produces("application/json")]
public sealed class StatusTransitionsController(
    IStatusTransitionsService transitionsService,
    IValidator<CreateStatusTransitionRequest> createTransitionValidator)
    : ControllerBase
{
    /// <summary>Lists a work item type's status transitions.</summary>
    /// <param name="typeId">The type's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The type's transitions.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetStatusTransitionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTransitions(Guid typeId, CancellationToken ct)
    {
        var result = await transitionsService.ListTransitionsForType(typeId, ct);

        return result switch
        {
            { HasError: false } => Ok(new GetStatusTransitionsResponse(result.Data.Select(x => x.ToPublicModel()))),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Creates a new status transition. Both statuses must already be ones the type uses.</summary>
    /// <param name="typeId">The type's id.</param>
    /// <param name="request">The transition to create.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">The transition was created.</response>
    /// <response code="400">The request failed validation, or the from/to status is not one of the type's statuses, or they are the same status.</response>
    /// <response code="404">No work item type exists with the given id.</response>
    /// <response code="409">This transition already exists for the type.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost]
    [ProducesResponseType(typeof(GetStatusTransitionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTransition(Guid typeId, CreateStatusTransitionRequest request, CancellationToken ct)
    {
        await createTransitionValidator.ValidateAndThrowAsync(request, ct);

        var transition = request.ToCoreModel(typeId);
        var result = await transitionsService.CreateTransition(transition, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data.ToPublicModel()),
            { HasError: true, Error: TplWorkItemNotFoundError } => NotFound(ErrorResponses.TplWorkItemNotFoundError),
            { HasError: true, Error: SameStatusTransitionError } => BadRequest(ErrorResponses.SameStatusTransitionError),
            { HasError: true, Error: StatusNotInTypeWorkflowError } => BadRequest(ErrorResponses.StatusNotInTypeWorkflowError),
            { HasError: true, Error: StatusTransitionAlreadyExistsError e } => Conflict(ErrorResponses.StatusTransitionAlreadyExistsError(e.ExistingTransitionId)),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Deletes a status transition. Idempotent: deleting a transition that doesn't exist (or was already deleted) still succeeds.</summary>
    /// <param name="typeId">The type's id.</param>
    /// <param name="id">The transition's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">The transition is deleted (or was already gone).</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTransition(Guid typeId, Guid id, CancellationToken ct)
    {
        var result = await transitionsService.SoftDeleteTransition(id, ct);

        return result switch
        {
            { HasError: false } => NoContent(),
            _ => throw new UnhandledResultErrorException(),
        };
    }
}
