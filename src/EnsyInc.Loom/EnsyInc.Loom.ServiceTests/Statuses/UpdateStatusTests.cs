using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Auth;
using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.Statuses;

[Collection(ApiCollectionDefinition.Name)]
public sealed class UpdateStatusTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task UpdateStatus_ValidBody_ReflectsInSubsequentGet()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateStatus($"Before-Update-{Guid.NewGuid()}", StatusCategory.ToDo, ct);
        var newName = $"After-Update-{Guid.NewGuid()}";

        var putResponse = await Fixture.Client.PutAsJsonAsync($"/statuses/{created.Id}", new UpdateStatusRequest(newName, StatusCategory.Done), ct);

        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
        var updated = await putResponse.Content.ReadFromJsonAsync<GetStatusResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(newName, updated!.Name);
        Assert.Equal(StatusCategory.Done, updated.Category);

        var getResponse = await Fixture.Client.GetAsync($"/statuses/{created.Id}", ct);
        var fetched = await getResponse.Content.ReadFromJsonAsync<GetStatusResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(newName, fetched!.Name);
        Assert.Equal(StatusCategory.Done, fetched.Category);
    }

    [Fact]
    public async Task UpdateStatus_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.PutAsJsonAsync($"/statuses/{Guid.NewGuid()}", new UpdateStatusRequest("Ghost", StatusCategory.ToDo), ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("TplWorkItemStatusNotFound", error!.ErrorCode);
    }

    [Fact]
    public async Task UpdateStatus_NoTokenOrBadToken_ReturnsUnauthorized()
    {
        var ct = TestContext.Current.CancellationToken;
        await AuthAssertions.AssertRequiresAuthentication(Fixture, HttpMethod.Put, $"/statuses/{Guid.NewGuid()}", ct);
    }
}
