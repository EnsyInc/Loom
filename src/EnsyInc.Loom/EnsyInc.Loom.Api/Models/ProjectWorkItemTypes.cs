using JetBrains.Annotations;

namespace EnsyInc.Loom.Api.Models;

/// <summary>The work item types a project has opted into.</summary>
/// <param name="WorkItemTypes">The types.</param>
[PublicAPI]
public sealed record GetProjectWorkItemTypesResponse(IEnumerable<GetWorkItemTypeResponse> WorkItemTypes);

/// <summary>The projects that have opted into a work item type.</summary>
/// <param name="Projects">The projects.</param>
[PublicAPI]
public sealed record GetWorkItemTypeProjectsResponse(IEnumerable<GetProjectResponse> Projects);
