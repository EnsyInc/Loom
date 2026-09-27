using EnsyInc.Loom.Api.Exceptions;
using EnsyInc.Loom.Api.Models;
using EnsyInc.Loom.Api.Models.Mappers;
using EnsyInc.Loom.Core.Errors;
using EnsyInc.Loom.Services.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnsyInc.Loom.Api.Controllers;

/// <summary>The signed-in user's own profile.</summary>
[ApiController]
[Authorize]
[Route("users")]
[Produces("application/json")]
public sealed class UsersController(IUsersService usersService) : ControllerBase
{
    /// <summary>Gets the signed-in user's profile.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The signed-in user.</response>
    /// <response code="404">The signed-in user has no matching profile.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        // Guaranteed present: JwtBearerEvents.OnTokenValidated already fails any token missing this claim,
        // so no request reaches here without it.
        var entraObjectId = User.FindFirst("oid")!.Value;
        var result = await usersService.GetByEntraObjectId(entraObjectId, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data.ToPublicModel()),
            { HasError: true, Error: UserNotFoundError } => NotFound(ErrorResponses.UserNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }
}
