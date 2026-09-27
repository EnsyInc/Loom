using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.WorkItemFields;

[Collection(ApiCollectionDefinition.Name)]
public sealed class CreateFieldTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task CreateField_ValidBody_ReturnsCreatedAndLocationRoundTrips()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        var key = $"priority-{Guid.NewGuid()}";

        var response = await Fixture.Client.PostAsJsonAsync($"/work-item-types/{type.Id}/fields", new CreateWorkItemFieldRequest(key, "Priority", WorkItemFieldDataType.Option, true, null), ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<GetWorkItemFieldResponse>(ApiFixture.JsonOptions, ct);
        Assert.NotNull(created);
        Assert.Equal(key, created!.Key);
        Assert.Equal(type.Id, created.TypeId);
        TrackCleanup(c => Fixture.Client.DeleteAsync($"/work-item-types/{type.Id}/fields/{created.Id}", c));

        Assert.NotNull(response.Headers.Location);
        var getResponse = await Fixture.Client.GetAsync(response.Headers.Location, ct);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task CreateField_NonexistentType_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.PostAsJsonAsync($"/work-item-types/{Guid.NewGuid()}/fields", new CreateWorkItemFieldRequest("key", "Label", WorkItemFieldDataType.Text, false, null), ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("TplWorkItemNotFound", error!.ErrorCode);
    }

    [Fact]
    public async Task CreateField_DuplicateKeyOnSameType_ReturnsConflict()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        var existing = await CreateField(type.Id, $"dup-{Guid.NewGuid()}", WorkItemFieldDataType.Text, false, null, ct);

        var response = await Fixture.Client.PostAsJsonAsync($"/work-item-types/{type.Id}/fields", new CreateWorkItemFieldRequest(existing.Key, "Other Label", WorkItemFieldDataType.Number, false, null), ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("TplWorkItemFieldKeyAlreadyExists", error!.ErrorCode);
        Assert.Equal(existing.Id.ToString(), error.Parameters["ExistingFieldId"]);
    }
}
