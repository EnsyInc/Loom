using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.WorkItemFieldOptions;

[Collection(ApiCollectionDefinition.Name)]
public sealed class CreateOptionTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task CreateOption_FieldIsOptionType_ReturnsOk()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        var field = await CreateField(type.Id, $"key-{Guid.NewGuid()}", WorkItemFieldDataType.MultiOption, false, null, ct);

        var response = await Fixture.Client.PostAsJsonAsync($"/fields/{field.Id}/options", new CreateFieldOptionRequest("low", "Low", 1), ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<GetFieldOptionResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("low", created!.Value);
        Assert.Equal(field.Id, created.FieldId);
        TrackCleanup(c => Fixture.Client.DeleteAsync($"/fields/{field.Id}/options/{created.Id}", c));
    }

    [Fact]
    public async Task CreateOption_FieldIsNotOptionType_ReturnsBadRequest()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        var field = await CreateField(type.Id, $"key-{Guid.NewGuid()}", WorkItemFieldDataType.Text, false, null, ct);

        var response = await Fixture.Client.PostAsJsonAsync($"/fields/{field.Id}/options", new CreateFieldOptionRequest("x", "X", 1), ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("FieldDoesNotSupportOptions", error!.ErrorCode);
    }

    [Fact]
    public async Task CreateOption_NonexistentField_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.PostAsJsonAsync($"/fields/{Guid.NewGuid()}/options", new CreateFieldOptionRequest("x", "X", 1), ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("TplWorkItemFieldNotFound", error!.ErrorCode);
    }

    [Fact]
    public async Task CreateOption_DuplicateValueOnSameField_ReturnsConflict()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        var field = await CreateField(type.Id, $"key-{Guid.NewGuid()}", WorkItemFieldDataType.Option, false, null, ct);
        var existing = await CreateOption(field.Id, "dup", 1, ct);

        var response = await Fixture.Client.PostAsJsonAsync($"/fields/{field.Id}/options", new CreateFieldOptionRequest(existing.Value, "Other Label", 2), ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("TplWorkItemFieldOptionValueAlreadyExists", error!.ErrorCode);
        Assert.Equal(existing.Id.ToString(), error.Parameters["ExistingOptionId"]);
    }
}
