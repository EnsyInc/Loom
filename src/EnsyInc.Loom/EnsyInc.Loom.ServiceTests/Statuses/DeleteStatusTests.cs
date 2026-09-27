using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Auth;
using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.Statuses;

[Collection(ApiCollectionDefinition.Name)]
public sealed class DeleteStatusTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task DeleteStatus_CalledTwice_BothReturnNoContent()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateStatus($"To-Delete-{Guid.NewGuid()}", StatusCategory.ToDo, ct);

        var firstDelete = await Fixture.Client.DeleteAsync($"/statuses/{created.Id}", ct);
        var secondDelete = await Fixture.Client.DeleteAsync($"/statuses/{created.Id}", ct);

        Assert.Equal(HttpStatusCode.NoContent, firstDelete.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, secondDelete.StatusCode);
    }

    [Fact]
    public async Task DeleteStatus_UsedByType_ReturnsConflict()
    {
        var ct = TestContext.Current.CancellationToken;
        var status = await CreateStatus($"InUse-{Guid.NewGuid()}", StatusCategory.ToDo, ct);
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        await AddStatusToType(type.Id, status.Id, ct);

        var response = await Fixture.Client.DeleteAsync($"/statuses/{status.Id}", ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("TplWorkItemStatusInUse", error!.ErrorCode);
    }

    [Fact]
    public async Task DeleteStatus_NoTokenOrBadToken_ReturnsUnauthorized()
    {
        var ct = TestContext.Current.CancellationToken;
        await AuthAssertions.AssertRequiresAuthentication(Fixture, HttpMethod.Delete, $"/statuses/{Guid.NewGuid()}", ct);
    }
}
