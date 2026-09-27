using System.Text.Json.Serialization;

using JetBrains.Annotations;

namespace EnsyInc.Loom.Api.Models;

/// <summary>Fields for creating a new option on a field.</summary>
/// <param name="Value">The option's stable identifier. Immutable after creation.</param>
/// <param name="Label">The option's display label.</param>
/// <param name="Rank">The option's order relative to the field's other options (e.g. Low &lt; Medium &lt; High).</param>
[PublicAPI]
public sealed record CreateFieldOptionRequest(
    string Value,
    string Label,
    [property: JsonRequired] int Rank);

/// <summary>Fields for updating an existing option's label and rank. Its value is immutable after creation.</summary>
/// <param name="Label">The option's display label.</param>
/// <param name="Rank">The option's order relative to the field's other options (e.g. Low &lt; Medium &lt; High).</param>
[PublicAPI]
public sealed record UpdateFieldOptionRequest(
    string Label,
    [property: JsonRequired] int Rank);

/// <summary>A single field option.</summary>
/// <param name="Id">The option's unique identifier.</param>
/// <param name="FieldId">The field this option belongs to.</param>
/// <param name="Value">The option's stable identifier.</param>
/// <param name="Label">The option's display label.</param>
/// <param name="Rank">The option's order relative to the field's other options.</param>
/// <param name="CreatedAt">When the option was created, in UTC.</param>
/// <param name="UpdatedAt">When the option was last updated, in UTC. Populated when the option is created.</param>
[PublicAPI]
public sealed record GetFieldOptionResponse(
    Guid Id,
    Guid FieldId,
    string Value,
    string Label,
    int Rank,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>A list of field options.</summary>
/// <param name="Options">The options.</param>
[PublicAPI]
public sealed record GetFieldOptionsResponse(IEnumerable<GetFieldOptionResponse> Options);
