using System.Net;

using EnsyInc.Loom.ServiceTests.Fixtures;

namespace EnsyInc.Loom.ServiceTests.Projects;

[Collection(ApiCollectionDefinition.Name)]
public sealed class DeleteProjectTests(ApiFixture fixture) : ProjectsApiTestBase(fixture)
{
    [Fact]
    public async Task DeleteProject_CalledTwice_BothReturnNoContent()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateProject($"To-Delete-{Guid.NewGuid()}", ct);

        var firstDelete = await Fixture.Client.DeleteAsync($"/projects/{created.Id}", ct);
        var secondDelete = await Fixture.Client.DeleteAsync($"/projects/{created.Id}", ct);

        Assert.Equal(HttpStatusCode.NoContent, firstDelete.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, secondDelete.StatusCode);
    }

    [Fact]
    public async Task DeleteProject_NonexistentId_ReturnsNoContent()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.DeleteAsync($"/projects/{Guid.NewGuid()}", ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProject_ThenGet_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateProject($"Deleted-Then-Get-{Guid.NewGuid()}", ct);

        var deleteResponse = await Fixture.Client.DeleteAsync($"/projects/{created.Id}", ct);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await Fixture.Client.GetAsync($"/projects/{created.Id}", ct);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
