using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Auth;
using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.WorkItemFields;

[Collection(ApiCollectionDefinition.Name)]
public sealed class GetFieldsTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task GetFields_TypeHasField_IncludesIt()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        var field = await CreateField(type.Id, $"priority-{Guid.NewGuid()}", WorkItemFieldDataType.Text, false, null, ct);

        var response = await Fixture.Client.GetAsync($"/work-item-types/{type.Id}/fields", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetWorkItemFieldsResponse>(ApiFixture.JsonOptions, ct);
        Assert.Contains(body!.Fields, f => f.Id == field.Id);
    }

    [Fact]
    public async Task GetFields_NoTokenOrBadToken_ReturnsUnauthorized()
    {
        var ct = TestContext.Current.CancellationToken;
        await AuthAssertions.AssertRequiresAuthentication(Fixture, HttpMethod.Get, $"/work-item-types/{Guid.NewGuid()}/fields", ct);
    }
}
