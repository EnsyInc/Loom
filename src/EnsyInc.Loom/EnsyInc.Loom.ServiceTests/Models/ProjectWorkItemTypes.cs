namespace EnsyInc.Loom.ServiceTests.Models;

public sealed record GetProjectWorkItemTypesResponse(IEnumerable<GetWorkItemTypeResponse> WorkItemTypes);

public sealed record GetWorkItemTypeProjectsResponse(IEnumerable<GetProjectResponse> Projects);
