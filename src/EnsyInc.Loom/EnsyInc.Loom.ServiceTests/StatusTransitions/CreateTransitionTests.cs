using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Auth;
using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.StatusTransitions;

[Collection(ApiCollectionDefinition.Name)]
public sealed class CreateTransitionTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task CreateTransition_BothStatusesUsedByType_ReturnsCreated()
    {
        var ct = TestContext.Current.CancellationToken;
        var status1 = await CreateStatus($"S1-{Guid.NewGuid()}", StatusCategory.ToDo, ct);
        var status2 = await CreateStatus($"S2-{Guid.NewGuid()}", StatusCategory.Done, ct);
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        await AddStatusToType(type.Id, status1.Id, ct);
        await AddStatusToType(type.Id, status2.Id, ct);

        var response = await Fixture.Client.PostAsJsonAsync($"/work-item-types/{type.Id}/transitions", new CreateStatusTransitionRequest(status1.Id, status2.Id), ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<GetStatusTransitionResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(type.Id, created!.TypeId);
        Assert.Equal(status1.Id, created.FromStatusId);
        Assert.Equal(status2.Id, created.ToStatusId);
    }

    [Fact]
    public async Task CreateTransition_SameFromAndTo_ReturnsBadRequest()
    {
        var ct = TestContext.Current.CancellationToken;
        var status = await CreateStatus($"Status-{Guid.NewGuid()}", StatusCategory.ToDo, ct);
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        await AddStatusToType(type.Id, status.Id, ct);

        var response = await Fixture.Client.PostAsJsonAsync($"/work-item-types/{type.Id}/transitions", new CreateStatusTransitionRequest(status.Id, status.Id), ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("SameStatusTransition", error!.ErrorCode);
    }

    [Fact]
    public async Task CreateTransition_StatusNotUsedByType_ReturnsBadRequest()
    {
        var ct = TestContext.Current.CancellationToken;
        var status1 = await CreateStatus($"S1-{Guid.NewGuid()}", StatusCategory.ToDo, ct);
        var status2 = await CreateStatus($"S2-{Guid.NewGuid()}", StatusCategory.Done, ct);
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        await AddStatusToType(type.Id, status1.Id, ct);

        var response = await Fixture.Client.PostAsJsonAsync($"/work-item-types/{type.Id}/transitions", new CreateStatusTransitionRequest(status1.Id, status2.Id), ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("StatusNotInTypeWorkflow", error!.ErrorCode);
    }

    [Fact]
    public async Task CreateTransition_DuplicateTransition_ReturnsConflict()
    {
        var ct = TestContext.Current.CancellationToken;
        var status1 = await CreateStatus($"S1-{Guid.NewGuid()}", StatusCategory.ToDo, ct);
        var status2 = await CreateStatus($"S2-{Guid.NewGuid()}", StatusCategory.Done, ct);
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        await AddStatusToType(type.Id, status1.Id, ct);
        await AddStatusToType(type.Id, status2.Id, ct);
        var existing = await CreateTransition(type.Id, status1.Id, status2.Id, ct);

        var response = await Fixture.Client.PostAsJsonAsync($"/work-item-types/{type.Id}/transitions", new CreateStatusTransitionRequest(status1.Id, status2.Id), ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("StatusTransitionAlreadyExists", error!.ErrorCode);
        Assert.Equal(existing.Id.ToString(), error.Parameters["ExistingTransitionId"]);
    }

    [Fact]
    public async Task CreateTransition_NoTokenOrBadToken_ReturnsUnauthorized()
    {
        var ct = TestContext.Current.CancellationToken;
        await AuthAssertions.AssertRequiresAuthentication(Fixture, HttpMethod.Post, $"/work-item-types/{Guid.NewGuid()}/transitions", ct);
    }
}
