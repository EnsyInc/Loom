using EnsyInc.Loom.Api.Exceptions;
using EnsyInc.Loom.Api.Models;
using EnsyInc.Loom.Api.Models.Mappers;
using EnsyInc.Loom.Core.Errors;
using EnsyInc.Loom.Services.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnsyInc.Loom.Api.Controllers;

/// <summary>Manage which statuses a work item type uses.</summary>
[ApiController]
[Authorize]
[Route("work-item-types/{typeId:guid}/statuses")]
[Produces("application/json")]
public sealed class WorkItemTypeStatusesController(IWorkItemTypeStatusesService typeStatusesService) : ControllerBase
{
    /// <summary>Lists the statuses a work item type uses.</summary>
    /// <param name="typeId">The type's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The type's statuses.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetStatusesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStatusesForType(Guid typeId, CancellationToken ct)
    {
        var result = await typeStatusesService.ListStatusesForType(typeId, ct);

        return result switch
        {
            { HasError: false } => Ok(new GetStatusesResponse(result.Data.Select(x => x.ToPublicModel()))),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Opts a work item type into a status. Idempotent: adding a status the type already uses still succeeds.</summary>
    /// <param name="typeId">The type's id.</param>
    /// <param name="statusId">The status's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">The type now uses the status.</response>
    /// <response code="404">No type or status exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPut("{statusId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddStatusToType(Guid typeId, Guid statusId, CancellationToken ct)
    {
        var result = await typeStatusesService.AddStatusToType(typeId, statusId, ct);

        return result switch
        {
            { HasError: false } => NoContent(),
            { HasError: true, Error: TplWorkItemNotFoundError } => NotFound(ErrorResponses.TplWorkItemNotFoundError),
            { HasError: true, Error: TplWorkItemStatusNotFoundError } => NotFound(ErrorResponses.TplWorkItemStatusNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>
    /// Removes a status from a work item type. Idempotent: removing a status the type doesn't use
    /// still succeeds. Fails if the status is the type's initial status, or if any transition for
    /// the type still references it.
    /// </summary>
    /// <param name="typeId">The type's id.</param>
    /// <param name="statusId">The status's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">The type no longer uses the status (or already didn't).</response>
    /// <response code="404">No type exists with the given id.</response>
    /// <response code="409">The status is the type's initial status, or still has transitions referencing it.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpDelete("{statusId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveStatusFromType(Guid typeId, Guid statusId, CancellationToken ct)
    {
        var result = await typeStatusesService.RemoveStatusFromType(typeId, statusId, ct);

        return result switch
        {
            { HasError: false } => NoContent(),
            { HasError: true, Error: TplWorkItemNotFoundError } => NotFound(ErrorResponses.TplWorkItemNotFoundError),
            { HasError: true, Error: InitialStatusCannotBeRemovedError } => Conflict(ErrorResponses.InitialStatusCannotBeRemovedError),
            { HasError: true, Error: StatusHasTransitionsError } => Conflict(ErrorResponses.StatusHasTransitionsError),
            _ => throw new UnhandledResultErrorException(),
        };
    }
}
