using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.WorkItemTypeStatuses;

[Collection(ApiCollectionDefinition.Name)]
public sealed class RemoveStatusFromTypeTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task RemoveStatusFromType_CalledTwice_BothReturnNoContent()
    {
        var ct = TestContext.Current.CancellationToken;
        var status = await CreateStatus($"Status-{Guid.NewGuid()}", StatusCategory.ToDo, ct);
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        await AddStatusToType(type.Id, status.Id, ct);

        var first = await Fixture.Client.DeleteAsync($"/work-item-types/{type.Id}/statuses/{status.Id}", ct);
        var second = await Fixture.Client.DeleteAsync($"/work-item-types/{type.Id}/statuses/{status.Id}", ct);

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);
    }

    [Fact]
    public async Task RemoveStatusFromType_NotLinked_ReturnsNoContent()
    {
        var ct = TestContext.Current.CancellationToken;
        var status = await CreateStatus($"NotLinked-{Guid.NewGuid()}", StatusCategory.ToDo, ct);
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);

        var response = await Fixture.Client.DeleteAsync($"/work-item-types/{type.Id}/statuses/{status.Id}", ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RemoveStatusFromType_IsInitialStatus_ReturnsConflict()
    {
        var ct = TestContext.Current.CancellationToken;
        var status = await CreateStatus($"Status-{Guid.NewGuid()}", StatusCategory.ToDo, ct);
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        await AddStatusToType(type.Id, status.Id, ct);
        await SetInitialStatus(type.Id, status.Id, ct);

        var response = await Fixture.Client.DeleteAsync($"/work-item-types/{type.Id}/statuses/{status.Id}", ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("InitialStatusCannotBeRemoved", error!.ErrorCode);
    }

    [Fact]
    public async Task RemoveStatusFromType_HasTransitions_ReturnsConflict()
    {
        var ct = TestContext.Current.CancellationToken;
        var status1 = await CreateStatus($"S1-{Guid.NewGuid()}", StatusCategory.ToDo, ct);
        var status2 = await CreateStatus($"S2-{Guid.NewGuid()}", StatusCategory.Done, ct);
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        await AddStatusToType(type.Id, status1.Id, ct);
        await AddStatusToType(type.Id, status2.Id, ct);
        await CreateTransition(type.Id, status1.Id, status2.Id, ct);

        var response = await Fixture.Client.DeleteAsync($"/work-item-types/{type.Id}/statuses/{status2.Id}", ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("StatusHasTransitions", error!.ErrorCode);
    }
}
