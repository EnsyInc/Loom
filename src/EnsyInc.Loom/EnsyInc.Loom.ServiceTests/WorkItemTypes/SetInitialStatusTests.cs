using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.WorkItemTypes;

[Collection(ApiCollectionDefinition.Name)]
public sealed class SetInitialStatusTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task SetInitialStatus_StatusUsedByType_UpdatesType()
    {
        var ct = TestContext.Current.CancellationToken;
        var status = await CreateStatus($"Status-{Guid.NewGuid()}", StatusCategory.ToDo, ct);
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        await AddStatusToType(type.Id, status.Id, ct);

        var response = await Fixture.Client.PutAsJsonAsync($"/work-item-types/{type.Id}/initial-status", new SetInitialStatusRequest(status.Id), ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<GetWorkItemTypeResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(status.Id, updated!.InitialStatusId);

        var getResponse = await Fixture.Client.GetAsync($"/work-item-types/{type.Id}", ct);
        var fetched = await getResponse.Content.ReadFromJsonAsync<GetWorkItemTypeResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(status.Id, fetched!.InitialStatusId);
    }

    [Fact]
    public async Task SetInitialStatus_StatusNotUsedByType_ReturnsBadRequest()
    {
        var ct = TestContext.Current.CancellationToken;
        var status = await CreateStatus($"NotLinked-{Guid.NewGuid()}", StatusCategory.ToDo, ct);
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);

        var response = await Fixture.Client.PutAsJsonAsync($"/work-item-types/{type.Id}/initial-status", new SetInitialStatusRequest(status.Id), ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("StatusNotInTypeWorkflow", error!.ErrorCode);
    }

    [Fact]
    public async Task SetInitialStatus_NonexistentType_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var status = await CreateStatus($"Status-{Guid.NewGuid()}", StatusCategory.ToDo, ct);

        var response = await Fixture.Client.PutAsJsonAsync($"/work-item-types/{Guid.NewGuid()}/initial-status", new SetInitialStatusRequest(status.Id), ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("TplWorkItemNotFound", error!.ErrorCode);
    }
}
