using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;

namespace EnsyInc.Loom.ServiceTests.Projects;

[Collection(ApiCollectionDefinition.Name)]
public sealed class GetProjectTests(ApiFixture fixture) : ProjectsApiTestBase(fixture)
{
    [Fact]
    public async Task GetProject_ExistingId_ReturnsProject()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateProject($"Get-Existing-{Guid.NewGuid()}", ct);

        var response = await Fixture.Client.GetAsync($"/projects/{created.Id}", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await response.Content.ReadFromJsonAsync<GetProjectResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal(created.Name, fetched.Name);
    }

    [Fact]
    public async Task GetProject_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.GetAsync($"/projects/{Guid.NewGuid()}", ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("ProjectNotFound", error!.ErrorCode);
    }
}
