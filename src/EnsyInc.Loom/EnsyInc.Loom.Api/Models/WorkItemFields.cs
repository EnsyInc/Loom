using System.Text.Json.Serialization;

using EnsyInc.Loom.Core.Models;

using JetBrains.Annotations;

namespace EnsyInc.Loom.Api.Models;

/// <summary>Fields for creating a new field on a work item type.</summary>
/// <param name="Key">The field's stable identifier. Immutable after creation.</param>
/// <param name="Label">The field's display label.</param>
/// <param name="DataType">The field's data type. Immutable after creation.</param>
/// <param name="Required">Whether the field must have a value.</param>
/// <param name="DefaultValue">The field's default value, as raw JSON, if any.</param>
[PublicAPI]
public sealed record CreateWorkItemFieldRequest(
    string Key,
    string Label,
    [property: JsonRequired] WorkItemFieldDataType DataType,
    [property: JsonRequired] bool Required,
    string? DefaultValue);

/// <summary>Fields for updating an existing field's label, required flag, and default value. Its key and data type are immutable after creation.</summary>
/// <param name="Label">The field's display label.</param>
/// <param name="Required">Whether the field must have a value.</param>
/// <param name="DefaultValue">The field's default value, as raw JSON, if any.</param>
[PublicAPI]
public sealed record UpdateWorkItemFieldRequest(
    string Label,
    [property: JsonRequired] bool Required,
    string? DefaultValue);

/// <summary>A single field.</summary>
/// <param name="Id">The field's unique identifier.</param>
/// <param name="TypeId">The work item type this field belongs to.</param>
/// <param name="Key">The field's stable identifier.</param>
/// <param name="Label">The field's display label.</param>
/// <param name="DataType">The field's data type.</param>
/// <param name="Required">Whether the field must have a value.</param>
/// <param name="DefaultValue">The field's default value, as raw JSON, if any.</param>
/// <param name="CreatedAt">When the field was created, in UTC.</param>
/// <param name="UpdatedAt">When the field was last updated, in UTC. Populated when the field is created.</param>
[PublicAPI]
public sealed record GetWorkItemFieldResponse(
    Guid Id,
    Guid TypeId,
    string Key,
    string Label,
    WorkItemFieldDataType DataType,
    bool Required,
    string? DefaultValue,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>A list of fields.</summary>
/// <param name="Fields">The fields.</param>
[PublicAPI]
public sealed record GetWorkItemFieldsResponse(IEnumerable<GetWorkItemFieldResponse> Fields);
