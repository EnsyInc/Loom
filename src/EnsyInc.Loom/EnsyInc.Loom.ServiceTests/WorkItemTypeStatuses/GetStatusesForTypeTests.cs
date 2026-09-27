using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Auth;
using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.WorkItemTypeStatuses;

[Collection(ApiCollectionDefinition.Name)]
public sealed class GetStatusesForTypeTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task GetStatusesForType_TypeUsesStatus_IncludesIt()
    {
        var ct = TestContext.Current.CancellationToken;
        var status = await CreateStatus($"Status-{Guid.NewGuid()}", StatusCategory.ToDo, ct);
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        await AddStatusToType(type.Id, status.Id, ct);

        var response = await Fixture.Client.GetAsync($"/work-item-types/{type.Id}/statuses", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetStatusesResponse>(ApiFixture.JsonOptions, ct);
        Assert.Contains(body!.Statuses, s => s.Id == status.Id);
    }

    [Fact]
    public async Task GetStatusesForType_TypeUsesNoStatuses_ReturnsEmpty()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);

        var response = await Fixture.Client.GetAsync($"/work-item-types/{type.Id}/statuses", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetStatusesResponse>(ApiFixture.JsonOptions, ct);
        Assert.Empty(body!.Statuses);
    }

    [Fact]
    public async Task GetStatusesForType_NoTokenOrBadToken_ReturnsUnauthorized()
    {
        var ct = TestContext.Current.CancellationToken;
        await AuthAssertions.AssertRequiresAuthentication(Fixture, HttpMethod.Get, $"/work-item-types/{Guid.NewGuid()}/statuses", ct);
    }
}
