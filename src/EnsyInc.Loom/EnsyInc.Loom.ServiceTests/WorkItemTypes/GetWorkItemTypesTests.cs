using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Auth;
using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.WorkItemTypes;

[Collection(ApiCollectionDefinition.Name)]
public sealed class GetWorkItemTypesTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task GetWorkItemTypes_IncludesCreatedType()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateType($"Listed-{Guid.NewGuid()}", null, ct);

        var response = await Fixture.Client.GetAsync("/work-item-types", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetWorkItemTypesResponse>(ApiFixture.JsonOptions, ct);
        Assert.Contains(body!.WorkItemTypes, t => t.Id == created.Id);
    }

    [Fact]
    public async Task GetWorkItemTypes_ExcludesDeletedType()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateType($"Listed-Then-Deleted-{Guid.NewGuid()}", null, ct);
        var deleteResponse = await Fixture.Client.DeleteAsync($"/work-item-types/{created.Id}", ct);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var response = await Fixture.Client.GetAsync("/work-item-types", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetWorkItemTypesResponse>(ApiFixture.JsonOptions, ct);
        Assert.DoesNotContain(body!.WorkItemTypes, t => t.Id == created.Id);
    }

    [Fact]
    public async Task GetWorkItemTypes_NoTokenOrBadToken_ReturnsUnauthorized()
    {
        var ct = TestContext.Current.CancellationToken;
        await AuthAssertions.AssertRequiresAuthentication(Fixture, HttpMethod.Get, "/work-item-types", ct);
    }
}
