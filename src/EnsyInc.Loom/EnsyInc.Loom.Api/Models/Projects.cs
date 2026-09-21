using JetBrains.Annotations;

namespace EnsyInc.Loom.Api.Models;

/// <summary>Fields for creating a new project.</summary>
/// <param name="Name">The project's display name.</param>
[PublicAPI]
public sealed record CreateProjectRequest(
    string Name);

/// <summary>A single project.</summary>
/// <param name="Id">The project's unique identifier.</param>
/// <param name="Name">The project's display name.</param>
/// <param name="CreatedAt">When the project was created, in UTC.</param>
/// <param name="UpdatedAt">When the project was last updated, in UTC. Populated when the project is created.</param>
[PublicAPI]
public sealed record GetProjectResponse(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>A list of projects.</summary>
/// <param name="Projects">The projects.</param>
[PublicAPI]
public sealed record GetProjectsResponse(IEnumerable<GetProjectResponse> Projects);

/// <summary>Fields for updating an existing project.</summary>
/// <param name="Name">The project's display name.</param>
[PublicAPI]
public sealed record UpdateProjectRequest(
    string Name);
