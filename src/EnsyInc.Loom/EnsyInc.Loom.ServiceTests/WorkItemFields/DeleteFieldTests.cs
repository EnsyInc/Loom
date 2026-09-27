using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.WorkItemFields;

[Collection(ApiCollectionDefinition.Name)]
public sealed class DeleteFieldTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task DeleteField_CalledTwice_BothReturnNoContent()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        var field = await CreateField(type.Id, $"key-{Guid.NewGuid()}", WorkItemFieldDataType.Text, false, null, ct);

        var firstDelete = await Fixture.Client.DeleteAsync($"/work-item-types/{type.Id}/fields/{field.Id}", ct);
        var secondDelete = await Fixture.Client.DeleteAsync($"/work-item-types/{type.Id}/fields/{field.Id}", ct);

        Assert.Equal(HttpStatusCode.NoContent, firstDelete.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, secondDelete.StatusCode);
    }

    [Fact]
    public async Task DeleteField_HasOptions_ReturnsConflict()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        var field = await CreateField(type.Id, $"key-{Guid.NewGuid()}", WorkItemFieldDataType.Option, false, null, ct);
        await CreateOption(field.Id, "high", 1, ct);

        var response = await Fixture.Client.DeleteAsync($"/work-item-types/{type.Id}/fields/{field.Id}", ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("TplWorkItemFieldInUse", error!.ErrorCode);
    }
}
