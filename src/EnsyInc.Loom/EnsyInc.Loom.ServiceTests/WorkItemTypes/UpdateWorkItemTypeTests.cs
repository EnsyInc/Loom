using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.WorkItemTypes;

[Collection(ApiCollectionDefinition.Name)]
public sealed class UpdateWorkItemTypeTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task UpdateWorkItemType_ValidBody_ReflectsInSubsequentGet()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateType($"Before-Update-{Guid.NewGuid()}", null, ct);
        var newName = $"After-Update-{Guid.NewGuid()}";

        var putResponse = await Fixture.Client.PutAsJsonAsync($"/work-item-types/{created.Id}", new UpdateWorkItemTypeRequest(newName, "https://example.com/new.png"), ct);

        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
        var updated = await putResponse.Content.ReadFromJsonAsync<GetWorkItemTypeResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(newName, updated!.Name);
        Assert.Equal("https://example.com/new.png", updated.IconUrl);

        var getResponse = await Fixture.Client.GetAsync($"/work-item-types/{created.Id}", ct);
        var fetched = await getResponse.Content.ReadFromJsonAsync<GetWorkItemTypeResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(newName, fetched!.Name);
    }

    [Fact]
    public async Task UpdateWorkItemType_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.PutAsJsonAsync($"/work-item-types/{Guid.NewGuid()}", new UpdateWorkItemTypeRequest("Ghost", null), ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("TplWorkItemNotFound", error!.ErrorCode);
    }
}
