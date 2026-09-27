namespace EnsyInc.Loom.ServiceTests.Models;

public sealed record CreateWorkItemFieldRequest(
    string Key,
    string Label,
    WorkItemFieldDataType DataType,
    bool Required,
    string? DefaultValue);

public sealed record UpdateWorkItemFieldRequest(
    string Label,
    bool Required,
    string? DefaultValue);

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

public sealed record GetWorkItemFieldsResponse(IEnumerable<GetWorkItemFieldResponse> Fields);
