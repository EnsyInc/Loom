namespace EnsyInc.Loom.ServiceTests.Models;

public sealed record CreateProjectRequest(string Name);

public sealed record UpdateProjectRequest(string Name);

public sealed record GetProjectResponse(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record GetProjectsResponse(IEnumerable<GetProjectResponse> Projects);
