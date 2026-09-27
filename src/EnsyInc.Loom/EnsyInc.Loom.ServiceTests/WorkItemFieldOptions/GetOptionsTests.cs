using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Auth;
using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.WorkItemFieldOptions;

[Collection(ApiCollectionDefinition.Name)]
public sealed class GetOptionsTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task GetOptions_FieldHasOption_IncludesIt()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        var field = await CreateField(type.Id, $"key-{Guid.NewGuid()}", WorkItemFieldDataType.Option, false, null, ct);
        var option = await CreateOption(field.Id, "high", 1, ct);

        var response = await Fixture.Client.GetAsync($"/fields/{field.Id}/options", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetFieldOptionsResponse>(ApiFixture.JsonOptions, ct);
        Assert.Contains(body!.Options, o => o.Id == option.Id);
    }

    [Fact]
    public async Task GetOptions_NoTokenOrBadToken_ReturnsUnauthorized()
    {
        var ct = TestContext.Current.CancellationToken;
        await AuthAssertions.AssertRequiresAuthentication(Fixture, HttpMethod.Get, $"/fields/{Guid.NewGuid()}/options", ct);
    }
}
