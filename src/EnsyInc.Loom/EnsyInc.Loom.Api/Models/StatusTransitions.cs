using System.Text.Json.Serialization;

using JetBrains.Annotations;

namespace EnsyInc.Loom.Api.Models;

/// <summary>Fields for creating a new status transition.</summary>
/// <param name="FromStatusId">The status the transition moves from. Must already be one of the type's statuses.</param>
/// <param name="ToStatusId">The status the transition moves to. Must already be one of the type's statuses.</param>
[PublicAPI]
public sealed record CreateStatusTransitionRequest(
    [property: JsonRequired] Guid FromStatusId,
    [property: JsonRequired] Guid ToStatusId);

/// <summary>A single status transition.</summary>
/// <param name="Id">The transition's unique identifier.</param>
/// <param name="TypeId">The work item type this transition belongs to.</param>
/// <param name="FromStatusId">The status the transition moves from.</param>
/// <param name="ToStatusId">The status the transition moves to.</param>
/// <param name="CreatedAt">When the transition was created, in UTC.</param>
/// <param name="UpdatedAt">When the transition was last updated, in UTC. Populated when the transition is created.</param>
[PublicAPI]
public sealed record GetStatusTransitionResponse(
    Guid Id,
    Guid TypeId,
    Guid FromStatusId,
    Guid ToStatusId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>A list of status transitions.</summary>
/// <param name="Transitions">The transitions.</param>
[PublicAPI]
public sealed record GetStatusTransitionsResponse(IEnumerable<GetStatusTransitionResponse> Transitions);
