using System.Text.Json.Serialization;

using EnsyInc.Loom.Core.Models;

using JetBrains.Annotations;

namespace EnsyInc.Loom.Api.Models;

/// <summary>Fields for creating a new status.</summary>
/// <param name="Name">The status's display name.</param>
/// <param name="Category">The status's category.</param>
[PublicAPI]
public sealed record CreateStatusRequest(
    string Name,
    [property: JsonRequired] StatusCategory Category);

/// <summary>Fields for updating an existing status.</summary>
/// <param name="Name">The status's display name.</param>
/// <param name="Category">The status's category.</param>
[PublicAPI]
public sealed record UpdateStatusRequest(
    string Name,
    [property: JsonRequired] StatusCategory Category);

/// <summary>A single status.</summary>
/// <param name="Id">The status's unique identifier.</param>
/// <param name="Name">The status's display name.</param>
/// <param name="Category">The status's category.</param>
/// <param name="CreatedAt">When the status was created, in UTC.</param>
/// <param name="UpdatedAt">When the status was last updated, in UTC. Populated when the status is created.</param>
[PublicAPI]
public sealed record GetStatusResponse(
    Guid Id,
    string Name,
    StatusCategory Category,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>A list of statuses.</summary>
/// <param name="Statuses">The statuses.</param>
[PublicAPI]
public sealed record GetStatusesResponse(IEnumerable<GetStatusResponse> Statuses);
