using System.Net;

using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.WorkItemFieldOptions;

[Collection(ApiCollectionDefinition.Name)]
public sealed class DeleteOptionTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task DeleteOption_CalledTwice_BothReturnNoContent()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        var field = await CreateField(type.Id, $"key-{Guid.NewGuid()}", WorkItemFieldDataType.Option, false, null, ct);
        var option = await CreateOption(field.Id, "high", 1, ct);

        var firstDelete = await Fixture.Client.DeleteAsync($"/fields/{field.Id}/options/{option.Id}", ct);
        var secondDelete = await Fixture.Client.DeleteAsync($"/fields/{field.Id}/options/{option.Id}", ct);

        Assert.Equal(HttpStatusCode.NoContent, firstDelete.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, secondDelete.StatusCode);
    }
}
