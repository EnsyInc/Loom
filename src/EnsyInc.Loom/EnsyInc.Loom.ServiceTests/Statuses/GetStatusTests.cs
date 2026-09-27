using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Auth;
using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.Statuses;

[Collection(ApiCollectionDefinition.Name)]
public sealed class GetStatusTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task GetStatus_ExistingId_ReturnsStatus()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateStatus($"Get-Existing-{Guid.NewGuid()}", StatusCategory.InProgress, ct);

        var response = await Fixture.Client.GetAsync($"/statuses/{created.Id}", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await response.Content.ReadFromJsonAsync<GetStatusResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal(created.Name, fetched.Name);
        Assert.Equal(StatusCategory.InProgress, fetched.Category);
    }

    [Fact]
    public async Task GetStatus_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.GetAsync($"/statuses/{Guid.NewGuid()}", ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("TplWorkItemStatusNotFound", error!.ErrorCode);
    }

    [Fact]
    public async Task GetStatus_NoTokenOrBadToken_ReturnsUnauthorized()
    {
        var ct = TestContext.Current.CancellationToken;
        await AuthAssertions.AssertRequiresAuthentication(Fixture, HttpMethod.Get, $"/statuses/{Guid.NewGuid()}", ct);
    }
}
