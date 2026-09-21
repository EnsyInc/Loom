using System.Collections.ObjectModel;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;

namespace EnsyInc.Loom.ServiceTests.Projects;

/// <summary>Shared project setup and cleanup for the per-endpoint Project test classes.</summary>
public abstract class ProjectsApiTestBase(ApiFixture fixture) : IAsyncDisposable
{
    protected ApiFixture Fixture { get; } = fixture;

    protected Collection<Guid> CreatedProjectIds { get; } = [];

    public async ValueTask DisposeAsync()
    {
        var ct = TestContext.Current.CancellationToken;
        foreach (var id in CreatedProjectIds)
        {
            await Fixture.Client.DeleteAsync($"/projects/{id}", ct);
        }

        GC.SuppressFinalize(this);
    }

    protected async Task<GetProjectResponse> CreateProject(string name, CancellationToken ct)
    {
        var response = await Fixture.Client.PostAsJsonAsync("/projects", new CreateProjectRequest(name), ct);
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<GetProjectResponse>(ApiFixture.JsonOptions, ct))!;
        CreatedProjectIds.Add(body.Id);
        return body;
    }
}
