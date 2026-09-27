using EnsyInc.Loom.Api.Exceptions;
using EnsyInc.Loom.Api.Models;
using EnsyInc.Loom.Api.Models.Mappers;
using EnsyInc.Loom.Core.Errors;
using EnsyInc.Loom.Services.Abstractions;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;

namespace EnsyInc.Loom.Api.Controllers;

/// <summary>Manage statuses, shared across work item types.</summary>
[ApiController]
[Route("statuses")]
[Produces("application/json")]
public sealed class TplWorkItemStatusesController(
    ITplWorkItemStatusesService statusesService,
    IValidator<CreateStatusRequest> createStatusValidator,
    IValidator<UpdateStatusRequest> updateStatusValidator)
    : ControllerBase
{
    /// <summary>Lists all statuses.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The statuses.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetStatusesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStatuses(CancellationToken ct)
    {
        var result = await statusesService.ListStatuses(ct);

        return result switch
        {
            { HasError: false } => Ok(new GetStatusesResponse(result.Data.Select(x => x.ToPublicModel()))),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Gets a single status by id.</summary>
    /// <param name="id">The status's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The status.</response>
    /// <response code="404">No status exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStatus(Guid id, CancellationToken ct)
    {
        var result = await statusesService.GetStatus(id, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data.ToPublicModel()),
            { HasError: true, Error: TplWorkItemStatusNotFoundError } => NotFound(ErrorResponses.TplWorkItemStatusNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Creates a new status.</summary>
    /// <param name="request">The status to create.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">The status was created. The response body and <c>Location</c> header describe the new status.</response>
    /// <response code="400">The request failed validation (e.g. a missing name).</response>
    /// <response code="409">A status with this name already exists.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost]
    [ProducesResponseType(typeof(GetStatusResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateStatus(CreateStatusRequest request, CancellationToken ct)
    {
        await createStatusValidator.ValidateAndThrowAsync(request, ct);

        var status = request.ToCoreModel();
        var result = await statusesService.CreateStatus(status, ct);

        return result switch
        {
            { HasError: false } => CreatedAtAction(nameof(GetStatus), new { id = result.Data.Id }, result.Data.ToPublicModel()),
            { HasError: true, Error: TplWorkItemStatusNameAlreadyExistsError e } => Conflict(ErrorResponses.TplWorkItemStatusNameAlreadyExistsError(e.ExistingStatusId)),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Updates an existing status.</summary>
    /// <param name="id">The status's id.</param>
    /// <param name="request">The new field values.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The updated status.</response>
    /// <response code="400">The request failed validation (e.g. a missing name).</response>
    /// <response code="404">No status exists with the given id.</response>
    /// <response code="409">A status with this name already exists.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(GetStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateStatusRequest request, CancellationToken ct)
    {
        await updateStatusValidator.ValidateAndThrowAsync(request, ct);

        var status = request.ToCoreModel(id);
        var result = await statusesService.UpdateStatus(status, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data.ToPublicModel()),
            { HasError: true, Error: TplWorkItemStatusNotFoundError } => NotFound(ErrorResponses.TplWorkItemStatusNotFoundError),
            { HasError: true, Error: TplWorkItemStatusNameAlreadyExistsError e } => Conflict(ErrorResponses.TplWorkItemStatusNameAlreadyExistsError(e.ExistingStatusId)),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Deletes a status. Idempotent: deleting a status that doesn't exist (or was already deleted) still succeeds.</summary>
    /// <param name="id">The status's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">The status is deleted (or was already gone).</response>
    /// <response code="409">The status is used by at least one work item type.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteStatus(Guid id, CancellationToken ct)
    {
        var result = await statusesService.SoftDeleteStatus(id, ct);

        return result switch
        {
            { HasError: false } => NoContent(),
            { HasError: true, Error: TplWorkItemStatusInUseError } => Conflict(ErrorResponses.TplWorkItemStatusInUseError),
            _ => throw new UnhandledResultErrorException(),
        };
    }
}
