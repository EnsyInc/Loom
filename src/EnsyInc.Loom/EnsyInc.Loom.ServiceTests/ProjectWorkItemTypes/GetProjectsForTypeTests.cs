using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Auth;
using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.ProjectWorkItemTypes;

[Collection(ApiCollectionDefinition.Name)]
public sealed class GetProjectsForTypeTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task GetProjectsForType_ProjectOptedIn_IncludesProject()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        var project = await CreateProject($"Project-{Guid.NewGuid()}", ct);
        await OptInProjectToType(project.Id, type.Id, ct);

        var response = await Fixture.Client.GetAsync($"/work-item-types/{type.Id}/projects", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetWorkItemTypeProjectsResponse>(ApiFixture.JsonOptions, ct);
        Assert.Contains(body!.Projects, p => p.Id == project.Id);
    }

    [Fact]
    public async Task GetProjectsForType_NoTokenOrBadToken_ReturnsUnauthorized()
    {
        var ct = TestContext.Current.CancellationToken;
        await AuthAssertions.AssertRequiresAuthentication(Fixture, HttpMethod.Get, $"/work-item-types/{Guid.NewGuid()}/projects", ct);
    }
}
