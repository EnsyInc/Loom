using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;

namespace EnsyInc.Loom.ServiceTests.Projects;

[Collection(ApiCollectionDefinition.Name)]
public sealed class GetProjectsTests(ApiFixture fixture) : ProjectsApiTestBase(fixture)
{
    [Fact]
    public async Task GetProjects_IncludesCreatedProject()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateProject($"Listed-{Guid.NewGuid()}", ct);

        var response = await Fixture.Client.GetAsync("/projects", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetProjectsResponse>(ApiFixture.JsonOptions, ct);
        Assert.Contains(body!.Projects, p => p.Id == created.Id);
    }

    [Fact]
    public async Task GetProjects_ExcludesDeletedProject()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateProject($"Listed-Then-Deleted-{Guid.NewGuid()}", ct);
        var deleteResponse = await Fixture.Client.DeleteAsync($"/projects/{created.Id}", ct);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var response = await Fixture.Client.GetAsync("/projects", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetProjectsResponse>(ApiFixture.JsonOptions, ct);
        Assert.DoesNotContain(body!.Projects, p => p.Id == created.Id);
    }
}
