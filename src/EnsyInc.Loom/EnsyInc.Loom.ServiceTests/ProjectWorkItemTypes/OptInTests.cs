using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.ProjectWorkItemTypes;

[Collection(ApiCollectionDefinition.Name)]
public sealed class OptInTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task OptIn_CalledTwice_IsIdempotent()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        var project = await CreateProject($"Project-{Guid.NewGuid()}", ct);

        var first = await Fixture.Client.PutAsync($"/projects/{project.Id}/work-item-types/{type.Id}", null, ct);
        var second = await Fixture.Client.PutAsync($"/projects/{project.Id}/work-item-types/{type.Id}", null, ct);

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);

        var listResponse = await Fixture.Client.GetAsync($"/projects/{project.Id}/work-item-types", ct);
        var body = await listResponse.Content.ReadFromJsonAsync<GetProjectWorkItemTypesResponse>(ApiFixture.JsonOptions, ct);
        Assert.Single(body!.WorkItemTypes, t => t.Id == type.Id);
    }

    [Fact]
    public async Task OptIn_NonexistentProject_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);

        var response = await Fixture.Client.PutAsync($"/projects/{Guid.NewGuid()}/work-item-types/{type.Id}", null, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("ProjectNotFound", error!.ErrorCode);
    }

    [Fact]
    public async Task OptIn_NonexistentType_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var project = await CreateProject($"Project-{Guid.NewGuid()}", ct);

        var response = await Fixture.Client.PutAsync($"/projects/{project.Id}/work-item-types/{Guid.NewGuid()}", null, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("TplWorkItemNotFound", error!.ErrorCode);
    }
}
