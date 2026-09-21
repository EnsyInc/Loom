using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;

namespace EnsyInc.Loom.ServiceTests.Projects;

[Collection(ApiCollectionDefinition.Name)]
public sealed class UpdateProjectTests(ApiFixture fixture) : ProjectsApiTestBase(fixture)
{
    [Fact]
    public async Task UpdateProject_ValidBody_ReflectsInSubsequentGet()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateProject($"Before-Update-{Guid.NewGuid()}", ct);
        var newName = $"After-Update-{Guid.NewGuid()}";

        var putResponse = await Fixture.Client.PutAsJsonAsync($"/projects/{created.Id}", new UpdateProjectRequest(newName), ct);

        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
        var updated = await putResponse.Content.ReadFromJsonAsync<GetProjectResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(newName, updated!.Name);
        Assert.NotNull(updated.UpdatedAt);

        var getResponse = await Fixture.Client.GetAsync($"/projects/{created.Id}", ct);
        var fetched = await getResponse.Content.ReadFromJsonAsync<GetProjectResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(newName, fetched!.Name);
    }

    [Fact]
    public async Task UpdateProject_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.PutAsJsonAsync($"/projects/{Guid.NewGuid()}", new UpdateProjectRequest("Ghost"), ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("ProjectNotFound", error!.ErrorCode);
    }

    [Fact]
    public async Task UpdateProject_EmptyName_ReturnsBadRequestAndLeavesProjectUnchanged()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateProject($"Keep-Name-{Guid.NewGuid()}", ct);

        var putResponse = await Fixture.Client.PutAsJsonAsync($"/projects/{created.Id}", new UpdateProjectRequest(string.Empty), ct);

        Assert.Equal(HttpStatusCode.BadRequest, putResponse.StatusCode);
        var error = await putResponse.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("ValidationError", error!.ErrorCode);

        var getResponse = await Fixture.Client.GetAsync($"/projects/{created.Id}", ct);
        var fetched = await getResponse.Content.ReadFromJsonAsync<GetProjectResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(created.Name, fetched!.Name);
    }
}
