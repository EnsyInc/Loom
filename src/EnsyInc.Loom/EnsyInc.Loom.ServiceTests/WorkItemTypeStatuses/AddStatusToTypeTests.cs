using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.WorkItemTypeStatuses;

[Collection(ApiCollectionDefinition.Name)]
public sealed class AddStatusToTypeTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task AddStatusToType_CalledTwice_IsIdempotent()
    {
        var ct = TestContext.Current.CancellationToken;
        var status = await CreateStatus($"Status-{Guid.NewGuid()}", StatusCategory.ToDo, ct);
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);

        var first = await Fixture.Client.PutAsync($"/work-item-types/{type.Id}/statuses/{status.Id}", null, ct);
        var second = await Fixture.Client.PutAsync($"/work-item-types/{type.Id}/statuses/{status.Id}", null, ct);

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);

        var listResponse = await Fixture.Client.GetAsync($"/work-item-types/{type.Id}/statuses", ct);
        var body = await listResponse.Content.ReadFromJsonAsync<GetStatusesResponse>(ApiFixture.JsonOptions, ct);
        Assert.Single(body!.Statuses, s => s.Id == status.Id);
    }

    [Fact]
    public async Task AddStatusToType_NonexistentType_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var status = await CreateStatus($"Status-{Guid.NewGuid()}", StatusCategory.ToDo, ct);

        var response = await Fixture.Client.PutAsync($"/work-item-types/{Guid.NewGuid()}/statuses/{status.Id}", null, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("TplWorkItemNotFound", error!.ErrorCode);
    }

    [Fact]
    public async Task AddStatusToType_NonexistentStatus_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);

        var response = await Fixture.Client.PutAsync($"/work-item-types/{type.Id}/statuses/{Guid.NewGuid()}", null, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("TplWorkItemStatusNotFound", error!.ErrorCode);
    }
}
