using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Auth;
using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.Statuses;

[Collection(ApiCollectionDefinition.Name)]
public sealed class CreateStatusTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task CreateStatus_ValidBody_ReturnsCreatedAndLocationRoundTrips()
    {
        var ct = TestContext.Current.CancellationToken;
        var name = $"ToDo-{Guid.NewGuid()}";

        var response = await Fixture.Client.PostAsJsonAsync("/statuses", new CreateStatusRequest(name, StatusCategory.ToDo), ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<GetStatusResponse>(ApiFixture.JsonOptions, ct);
        Assert.NotNull(created);
        Assert.Equal(name, created!.Name);
        Assert.Equal(StatusCategory.ToDo, created.Category);
        TrackCleanup(c => Fixture.Client.DeleteAsync($"/statuses/{created.Id}", c));

        Assert.NotNull(response.Headers.Location);
        var getResponse = await Fixture.Client.GetAsync(response.Headers.Location, ct);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task CreateStatus_EmptyName_ReturnsBadRequestWithValidationError()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.PostAsJsonAsync("/statuses", new CreateStatusRequest(string.Empty, StatusCategory.ToDo), ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("ValidationError", error!.ErrorCode);
        Assert.Contains("Name", error.Parameters.Keys);
    }

    [Fact]
    public async Task CreateStatus_DuplicateName_ReturnsConflict()
    {
        var ct = TestContext.Current.CancellationToken;
        var existing = await CreateStatus($"Dup-{Guid.NewGuid()}", StatusCategory.ToDo, ct);

        var response = await Fixture.Client.PostAsJsonAsync("/statuses", new CreateStatusRequest(existing.Name, StatusCategory.Done), ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("TplWorkItemStatusNameAlreadyExists", error!.ErrorCode);
        Assert.Equal(existing.Id.ToString(), error.Parameters["ExistingStatusId"]);
    }

    [Fact]
    public async Task CreateStatus_NoTokenOrBadToken_ReturnsUnauthorized()
    {
        var ct = TestContext.Current.CancellationToken;
        await AuthAssertions.AssertRequiresAuthentication(Fixture, HttpMethod.Post, "/statuses", ct);
    }
}
