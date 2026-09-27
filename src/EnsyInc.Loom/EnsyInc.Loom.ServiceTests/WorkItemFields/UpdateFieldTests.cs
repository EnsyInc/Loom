using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Auth;
using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.WorkItemFields;

[Collection(ApiCollectionDefinition.Name)]
public sealed class UpdateFieldTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task UpdateField_ValidBody_UpdatesLabelRequiredAndDefaultButKeepsKeyAndDataType()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        var created = await CreateField(type.Id, $"key-{Guid.NewGuid()}", WorkItemFieldDataType.Number, false, null, ct);

        var putResponse = await Fixture.Client.PutAsJsonAsync(
            $"/work-item-types/{type.Id}/fields/{created.Id}",
            new UpdateWorkItemFieldRequest("New Label", true, "99"),
            ct);

        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
        var updated = await putResponse.Content.ReadFromJsonAsync<GetWorkItemFieldResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("New Label", updated!.Label);
        Assert.True(updated.Required);
        Assert.Equal("99", updated.DefaultValue);
        Assert.Equal(created.Key, updated.Key);
        Assert.Equal(WorkItemFieldDataType.Number, updated.DataType);
    }

    [Fact]
    public async Task UpdateField_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);

        var response = await Fixture.Client.PutAsJsonAsync(
            $"/work-item-types/{type.Id}/fields/{Guid.NewGuid()}",
            new UpdateWorkItemFieldRequest("Label", false, null),
            ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("TplWorkItemFieldNotFound", error!.ErrorCode);
    }

    [Fact]
    public async Task UpdateField_NoTokenOrBadToken_ReturnsUnauthorized()
    {
        var ct = TestContext.Current.CancellationToken;
        await AuthAssertions.AssertRequiresAuthentication(Fixture, HttpMethod.Put, $"/work-item-types/{Guid.NewGuid()}/fields/{Guid.NewGuid()}", ct);
    }
}
