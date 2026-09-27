using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Auth;
using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.ProjectWorkItemTypes;

[Collection(ApiCollectionDefinition.Name)]
public sealed class GetTypesForProjectTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task GetTypesForProject_ProjectOptedIn_IncludesType()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        var project = await CreateProject($"Project-{Guid.NewGuid()}", ct);
        await OptInProjectToType(project.Id, type.Id, ct);

        var response = await Fixture.Client.GetAsync($"/projects/{project.Id}/work-item-types", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetProjectWorkItemTypesResponse>(ApiFixture.JsonOptions, ct);
        Assert.Contains(body!.WorkItemTypes, t => t.Id == type.Id);
    }

    [Fact]
    public async Task GetTypesForProject_ProjectNotOptedIntoAnyType_ReturnsEmpty()
    {
        var ct = TestContext.Current.CancellationToken;
        var project = await CreateProject($"Project-{Guid.NewGuid()}", ct);

        var response = await Fixture.Client.GetAsync($"/projects/{project.Id}/work-item-types", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetProjectWorkItemTypesResponse>(ApiFixture.JsonOptions, ct);
        Assert.Empty(body!.WorkItemTypes);
    }

    [Fact]
    public async Task GetTypesForProject_NoTokenOrBadToken_ReturnsUnauthorized()
    {
        var ct = TestContext.Current.CancellationToken;
        await AuthAssertions.AssertRequiresAuthentication(Fixture, HttpMethod.Get, $"/projects/{Guid.NewGuid()}/work-item-types", ct);
    }
}
