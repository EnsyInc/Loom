using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.WorkItemTypes;

[Collection(ApiCollectionDefinition.Name)]
public sealed class CreateWorkItemTypeTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task CreateWorkItemType_ValidBody_ReturnsCreatedWithNoInitialStatus()
    {
        var ct = TestContext.Current.CancellationToken;
        var name = $"Bug-{Guid.NewGuid()}";

        var response = await Fixture.Client.PostAsJsonAsync("/work-item-types", new CreateWorkItemTypeRequest(name, "https://example.com/icon.png"), ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<GetWorkItemTypeResponse>(ApiFixture.JsonOptions, ct);
        Assert.NotNull(created);
        Assert.Equal(name, created!.Name);
        Assert.Null(created.InitialStatusId);
        TrackCleanup(c => Fixture.Client.DeleteAsync($"/work-item-types/{created.Id}", c));

        Assert.NotNull(response.Headers.Location);
        var getResponse = await Fixture.Client.GetAsync(response.Headers.Location, ct);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task CreateWorkItemType_EmptyName_ReturnsBadRequestWithValidationError()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.PostAsJsonAsync("/work-item-types", new CreateWorkItemTypeRequest(string.Empty, null), ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("ValidationError", error!.ErrorCode);
    }

    [Fact]
    public async Task CreateWorkItemType_DuplicateName_ReturnsConflict()
    {
        var ct = TestContext.Current.CancellationToken;
        var existing = await CreateType($"Dup-{Guid.NewGuid()}", null, ct);

        var response = await Fixture.Client.PostAsJsonAsync("/work-item-types", new CreateWorkItemTypeRequest(existing.Name, null), ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("TplWorkItemNameAlreadyExists", error!.ErrorCode);
        Assert.Equal(existing.Id.ToString(), error.Parameters["ExistingTypeId"]);
    }
}
