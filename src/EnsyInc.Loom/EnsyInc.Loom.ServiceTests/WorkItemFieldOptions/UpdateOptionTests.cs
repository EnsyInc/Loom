using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Auth;
using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.WorkItemFieldOptions;

[Collection(ApiCollectionDefinition.Name)]
public sealed class UpdateOptionTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task UpdateOption_ValidBody_UpdatesLabelAndRankButKeepsValue()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        var field = await CreateField(type.Id, $"key-{Guid.NewGuid()}", WorkItemFieldDataType.Option, false, null, ct);
        var created = await CreateOption(field.Id, "high", 1, ct);

        var putResponse = await Fixture.Client.PutAsJsonAsync($"/fields/{field.Id}/options/{created.Id}", new UpdateFieldOptionRequest("Very High", 5), ct);

        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
        var updated = await putResponse.Content.ReadFromJsonAsync<GetFieldOptionResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("Very High", updated!.Label);
        Assert.Equal(5, updated.Rank);
        Assert.Equal(created.Value, updated.Value);
    }

    [Fact]
    public async Task UpdateOption_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        var field = await CreateField(type.Id, $"key-{Guid.NewGuid()}", WorkItemFieldDataType.Option, false, null, ct);

        var response = await Fixture.Client.PutAsJsonAsync($"/fields/{field.Id}/options/{Guid.NewGuid()}", new UpdateFieldOptionRequest("Label", 1), ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("TplWorkItemFieldOptionNotFound", error!.ErrorCode);
    }

    [Fact]
    public async Task UpdateOption_NoTokenOrBadToken_ReturnsUnauthorized()
    {
        var ct = TestContext.Current.CancellationToken;
        await AuthAssertions.AssertRequiresAuthentication(Fixture, HttpMethod.Put, $"/fields/{Guid.NewGuid()}/options/{Guid.NewGuid()}", ct);
    }
}
