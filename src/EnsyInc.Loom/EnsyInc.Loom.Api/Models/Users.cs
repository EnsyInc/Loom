using JetBrains.Annotations;

namespace EnsyInc.Loom.Api.Models;

/// <summary>The signed-in user's profile.</summary>
/// <param name="Id">The user's unique identifier.</param>
/// <param name="FirstName">The user's first name.</param>
/// <param name="LastName">The user's last name.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="CreatedAt">When the user first signed in, in UTC.</param>
/// <param name="UpdatedAt">When the user's profile was last refreshed, in UTC. Populated when the user is created.</param>
[PublicAPI]
public sealed record GetUserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
