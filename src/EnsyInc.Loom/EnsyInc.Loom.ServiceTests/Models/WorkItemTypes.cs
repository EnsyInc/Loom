namespace EnsyInc.Loom.ServiceTests.Models;

public sealed record CreateWorkItemTypeRequest(string Name, string? IconUrl);

public sealed record UpdateWorkItemTypeRequest(string Name, string? IconUrl);

public sealed record SetInitialStatusRequest(Guid StatusId);

public sealed record GetWorkItemTypeResponse(
    Guid Id,
    string Name,
    string? IconUrl,
    Guid? InitialStatusId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record GetWorkItemTypesResponse(IEnumerable<GetWorkItemTypeResponse> WorkItemTypes);
