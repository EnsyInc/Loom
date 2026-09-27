using System.Net;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;
using EnsyInc.Loom.ServiceTests.WorkItemTemplates;

namespace EnsyInc.Loom.ServiceTests.WorkItemFields;

[Collection(ApiCollectionDefinition.Name)]
public sealed class GetFieldTests(ApiFixture fixture) : WorkItemTemplatesApiTestBase(fixture)
{
    [Fact]
    public async Task GetField_ExistingId_ReturnsField()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);
        var created = await CreateField(type.Id, $"key-{Guid.NewGuid()}", WorkItemFieldDataType.Number, true, "42", ct);

        var response = await Fixture.Client.GetAsync($"/work-item-types/{type.Id}/fields/{created.Id}", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await response.Content.ReadFromJsonAsync<GetWorkItemFieldResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal(created.Key, fetched.Key);
        Assert.Equal(WorkItemFieldDataType.Number, fetched.DataType);
        Assert.True(fetched.Required);
        Assert.Equal("42", fetched.DefaultValue);
    }

    [Fact]
    public async Task GetField_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var type = await CreateType($"Type-{Guid.NewGuid()}", null, ct);

        var response = await Fixture.Client.GetAsync($"/work-item-types/{type.Id}/fields/{Guid.NewGuid()}", ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("TplWorkItemFieldNotFound", error!.ErrorCode);
    }
}
