using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Auth;
using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.WorkItemTypes;

[Collection(ApiCollectionDefinition.Name)]
public sealed class DeleteWorkItemTypeTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task DeleteWorkItemType_CalledTwice_BothReturnNoContent()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateType($"To-Delete-{Guid.NewGuid()}", null, ct);

        var firstDelete = await Fixture.Client.DeleteAsync($"/work-item-types/{created.Id}", ct);
        var secondDelete = await Fixture.Client.DeleteAsync($"/work-item-types/{created.Id}", ct);

        Assert.Equal(HttpStatusCode.NoContent, firstDelete.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, secondDelete.StatusCode);
    }

    [Fact]
    public async Task DeleteWorkItemType_UsedByProject_ReturnsConflict()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"InUse-{Guid.NewGuid()}", null, ct);
        var project = await CreateProject($"Project-{Guid.NewGuid()}", ct);
        await OptInProjectToType(project.Id, type.Id, ct);

        var response = await Fixture.Client.DeleteAsync($"/work-item-types/{type.Id}", ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("TplWorkItemInUse", error!.ErrorCode);
    }

    [Fact]
    public async Task DeleteWorkItemType_HasStatusesAndTransitions_CascadesSoTheyCanBeDeletedAfter()
    {
        var ct = TestContext.Current.CancellationToken;
        var status1 = await CreateStatus($"S1-{Guid.NewGuid()}", StatusCategory.ToDo, ct);
        var status2 = await CreateStatus($"S2-{Guid.NewGuid()}", StatusCategory.Done, ct);
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        await AddStatusToType(type.Id, status1.Id, ct);
        await AddStatusToType(type.Id, status2.Id, ct);
        await SetInitialStatus(type.Id, status1.Id, ct);
        await CreateTransition(type.Id, status1.Id, status2.Id, ct);

        var deleteTypeResponse = await Fixture.Client.DeleteAsync($"/work-item-types/{type.Id}", ct);
        Assert.Equal(HttpStatusCode.NoContent, deleteTypeResponse.StatusCode);

        // If the type's own status/transition links weren't cascade-deleted, these would still be
        // blocked as "in use" by the now-deleted type (see the cascade-soft-delete-and-bulk-delete-error memory).
        var deleteStatus1Response = await Fixture.Client.DeleteAsync($"/statuses/{status1.Id}", ct);
        var deleteStatus2Response = await Fixture.Client.DeleteAsync($"/statuses/{status2.Id}", ct);

        Assert.Equal(HttpStatusCode.NoContent, deleteStatus1Response.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, deleteStatus2Response.StatusCode);
    }

    [Fact]
    public async Task DeleteWorkItemType_NoTokenOrBadToken_ReturnsUnauthorized()
    {
        var ct = TestContext.Current.CancellationToken;
        await AuthAssertions.AssertRequiresAuthentication(Fixture, HttpMethod.Delete, $"/work-item-types/{Guid.NewGuid()}", ct);
    }
}
