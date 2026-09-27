using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.Statuses;

[Collection(ApiCollectionDefinition.Name)]
public sealed class GetStatusesTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task GetStatuses_IncludesCreatedStatus()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateStatus($"Listed-{Guid.NewGuid()}", StatusCategory.ToDo, ct);

        var response = await Fixture.Client.GetAsync("/statuses", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetStatusesResponse>(ApiFixture.JsonOptions, ct);
        Assert.Contains(body!.Statuses, s => s.Id == created.Id);
    }

    [Fact]
    public async Task GetStatuses_ExcludesDeletedStatus()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateStatus($"Listed-Then-Deleted-{Guid.NewGuid()}", StatusCategory.ToDo, ct);
        var deleteResponse = await Fixture.Client.DeleteAsync($"/statuses/{created.Id}", ct);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var response = await Fixture.Client.GetAsync("/statuses", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetStatusesResponse>(ApiFixture.JsonOptions, ct);
        Assert.DoesNotContain(body!.Statuses, s => s.Id == created.Id);
    }
}
