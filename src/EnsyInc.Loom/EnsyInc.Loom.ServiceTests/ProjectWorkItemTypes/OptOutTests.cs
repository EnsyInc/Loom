using System.Net;

using EnsyInc.Loom.ServiceTests.Auth;
using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.ProjectWorkItemTypes;

[Collection(ApiCollectionDefinition.Name)]
public sealed class OptOutTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task OptOut_CalledTwice_BothReturnNoContent()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        var project = await CreateProject($"Project-{Guid.NewGuid()}", ct);
        await OptInProjectToType(project.Id, type.Id, ct);

        var first = await Fixture.Client.DeleteAsync($"/projects/{project.Id}/work-item-types/{type.Id}", ct);
        var second = await Fixture.Client.DeleteAsync($"/projects/{project.Id}/work-item-types/{type.Id}", ct);

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);
    }

    [Fact]
    public async Task OptOut_UnblocksTypeDeletion()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        var project = await CreateProject($"Project-{Guid.NewGuid()}", ct);
        await OptInProjectToType(project.Id, type.Id, ct);

        var optOutResponse = await Fixture.Client.DeleteAsync($"/projects/{project.Id}/work-item-types/{type.Id}", ct);
        Assert.Equal(HttpStatusCode.NoContent, optOutResponse.StatusCode);

        var deleteTypeResponse = await Fixture.Client.DeleteAsync($"/work-item-types/{type.Id}", ct);
        Assert.Equal(HttpStatusCode.NoContent, deleteTypeResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteProject_UnblocksTypeDeletion()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        var project = await CreateProject($"Project-{Guid.NewGuid()}", ct);
        await OptInProjectToType(project.Id, type.Id, ct);

        // If Project delete didn't cascade-remove its opt-in link (see the
        // cascade-soft-delete-and-bulk-delete-error memory), the type would stay blocked forever.
        var deleteProjectResponse = await Fixture.Client.DeleteAsync($"/projects/{project.Id}", ct);
        Assert.Equal(HttpStatusCode.NoContent, deleteProjectResponse.StatusCode);

        var deleteTypeResponse = await Fixture.Client.DeleteAsync($"/work-item-types/{type.Id}", ct);
        Assert.Equal(HttpStatusCode.NoContent, deleteTypeResponse.StatusCode);
    }

    [Fact]
    public async Task OptOut_NoTokenOrBadToken_ReturnsUnauthorized()
    {
        var ct = TestContext.Current.CancellationToken;
        await AuthAssertions.AssertRequiresAuthentication(Fixture, HttpMethod.Delete, $"/projects/{Guid.NewGuid()}/work-item-types/{Guid.NewGuid()}", ct);
    }
}
