using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Auth;
using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.StatusTransitions;

[Collection(ApiCollectionDefinition.Name)]
public sealed class GetTransitionsTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task GetTransitions_TypeHasTransition_IncludesIt()
    {
        var ct = TestContext.Current.CancellationToken;
        var status1 = await CreateStatus($"S1-{Guid.NewGuid()}", StatusCategory.ToDo, ct);
        var status2 = await CreateStatus($"S2-{Guid.NewGuid()}", StatusCategory.Done, ct);
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        await AddStatusToType(type.Id, status1.Id, ct);
        await AddStatusToType(type.Id, status2.Id, ct);
        var transition = await CreateTransition(type.Id, status1.Id, status2.Id, ct);

        var response = await Fixture.Client.GetAsync($"/work-item-types/{type.Id}/transitions", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetStatusTransitionsResponse>(ApiFixture.JsonOptions, ct);
        Assert.Contains(body!.Transitions, t => t.Id == transition.Id);
    }

    [Fact]
    public async Task GetTransitions_TypeHasNoTransitions_ReturnsEmpty()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);

        var response = await Fixture.Client.GetAsync($"/work-item-types/{type.Id}/transitions", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetStatusTransitionsResponse>(ApiFixture.JsonOptions, ct);
        Assert.Empty(body!.Transitions);
    }

    [Fact]
    public async Task GetTransitions_NoTokenOrBadToken_ReturnsUnauthorized()
    {
        var ct = TestContext.Current.CancellationToken;
        await AuthAssertions.AssertRequiresAuthentication(Fixture, HttpMethod.Get, $"/work-item-types/{Guid.NewGuid()}/transitions", ct);
    }
}
