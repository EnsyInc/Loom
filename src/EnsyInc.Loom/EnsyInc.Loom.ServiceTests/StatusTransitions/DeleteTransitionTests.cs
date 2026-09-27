using System.Net;

using EnsyInc.Loom.ServiceTests.Auth;
using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.StatusTransitions;

[Collection(ApiCollectionDefinition.Name)]
public sealed class DeleteTransitionTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task DeleteTransition_CalledTwice_BothReturnNoContent()
    {
        var ct = TestContext.Current.CancellationToken;
        var status1 = await CreateStatus($"S1-{Guid.NewGuid()}", StatusCategory.ToDo, ct);
        var status2 = await CreateStatus($"S2-{Guid.NewGuid()}", StatusCategory.Done, ct);
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        await AddStatusToType(type.Id, status1.Id, ct);
        await AddStatusToType(type.Id, status2.Id, ct);
        var transition = await CreateTransition(type.Id, status1.Id, status2.Id, ct);

        var first = await Fixture.Client.DeleteAsync($"/work-item-types/{type.Id}/transitions/{transition.Id}", ct);
        var second = await Fixture.Client.DeleteAsync($"/work-item-types/{type.Id}/transitions/{transition.Id}", ct);

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);
    }

    [Fact]
    public async Task DeleteTransition_NonexistentId_ReturnsNoContent()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);

        var response = await Fixture.Client.DeleteAsync($"/work-item-types/{type.Id}/transitions/{Guid.NewGuid()}", ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTransition_NoTokenOrBadToken_ReturnsUnauthorized()
    {
        var ct = TestContext.Current.CancellationToken;
        await AuthAssertions.AssertRequiresAuthentication(Fixture, HttpMethod.Delete, $"/work-item-types/{Guid.NewGuid()}/transitions/{Guid.NewGuid()}", ct);
    }
}
