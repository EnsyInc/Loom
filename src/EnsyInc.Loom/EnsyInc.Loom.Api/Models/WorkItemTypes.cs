using System.Text.Json.Serialization;

using JetBrains.Annotations;

namespace EnsyInc.Loom.Api.Models;

/// <summary>Fields for creating a new work item type. It has no initial status yet — set one with <see cref="SetInitialStatusRequest"/> once it has at least one status.</summary>
/// <param name="Name">The type's display name.</param>
/// <param name="IconUrl">The type's icon, if any.</param>
[PublicAPI]
public sealed record CreateWorkItemTypeRequest(
    string Name,
    string? IconUrl);

/// <summary>Fields for updating an existing work item type's name and icon. Its initial status is unaffected — use <see cref="SetInitialStatusRequest"/> for that.</summary>
/// <param name="Name">The type's display name.</param>
/// <param name="IconUrl">The type's icon, if any.</param>
[PublicAPI]
public sealed record UpdateWorkItemTypeRequest(
    string Name,
    string? IconUrl);

/// <summary>Fields for setting the status new items of a type start in.</summary>
/// <param name="StatusId">The status, which must already be one of the type's statuses.</param>
[PublicAPI]
public sealed record SetInitialStatusRequest(
    [property: JsonRequired] Guid StatusId);

/// <summary>A single work item type.</summary>
/// <param name="Id">The type's unique identifier.</param>
/// <param name="Name">The type's display name.</param>
/// <param name="IconUrl">The type's icon, if any.</param>
/// <param name="InitialStatusId">The status new items of this type start in. Null until the type has been given at least one status and an initial status has been set.</param>
/// <param name="CreatedAt">When the type was created, in UTC.</param>
/// <param name="UpdatedAt">When the type was last updated, in UTC. Populated when the type is created.</param>
[PublicAPI]
public sealed record GetWorkItemTypeResponse(
    Guid Id,
    string Name,
    string? IconUrl,
    Guid? InitialStatusId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>A list of work item types.</summary>
/// <param name="WorkItemTypes">The work item types.</param>
[PublicAPI]
public sealed record GetWorkItemTypesResponse(IEnumerable<GetWorkItemTypeResponse> WorkItemTypes);
