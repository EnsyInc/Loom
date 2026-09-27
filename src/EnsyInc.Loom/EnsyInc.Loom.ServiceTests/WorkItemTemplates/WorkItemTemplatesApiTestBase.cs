using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;

namespace EnsyInc.Loom.ServiceTests.WorkItemTemplates;

/// <summary>
/// Shared setup and cleanup for the work item type template resources (statuses, types, their
/// links, transitions, fields, and field options), which are too interdependent for one
/// resource-per-base-class split like <c>ProjectsApiTestBase</c>. Cleanups run in reverse
/// creation order on dispose, so children are always deleted before the parents they reference.
/// </summary>
public abstract class WorkItemTemplatesApiTestBase(ApiFixture fixture) : IAsyncDisposable
{
    protected ApiFixture Fixture { get; } = fixture;

    private readonly Stack<Func<CancellationToken, Task>> _cleanups = new();

    public async ValueTask DisposeAsync()
    {
        var ct = TestContext.Current.CancellationToken;
        while (_cleanups.TryPop(out var cleanup))
        {
            await cleanup(ct);
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Registers a cleanup to run on dispose. For tests that create a resource via a raw
    /// <see cref="Fixture"/> call (rather than one of the <c>CreateXxx</c> helpers below) in order
    /// to assert on the creation response itself, e.g. <c>TrackCleanup(c => Fixture.Client.DeleteAsync($"/statuses/{created.Id}", c))</c>.
    /// </summary>
    protected void TrackCleanup(Func<CancellationToken, Task> cleanup) => _cleanups.Push(cleanup);

    protected async Task<GetStatusResponse> CreateStatus(string name, StatusCategory category, CancellationToken ct)
    {
        var response = await Fixture.Client.PostAsJsonAsync("/statuses", new CreateStatusRequest(name, category), ct);
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<GetStatusResponse>(ApiFixture.JsonOptions, ct))!;
        _cleanups.Push(c => Fixture.Client.DeleteAsync($"/statuses/{body.Id}", c));
        return body;
    }

    protected async Task<GetWorkItemTypeResponse> CreateType(string name, string? iconUrl, CancellationToken ct)
    {
        var response = await Fixture.Client.PostAsJsonAsync("/work-item-types", new CreateWorkItemTypeRequest(name, iconUrl), ct);
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<GetWorkItemTypeResponse>(ApiFixture.JsonOptions, ct))!;
        _cleanups.Push(c => Fixture.Client.DeleteAsync($"/work-item-types/{body.Id}", c));
        return body;
    }

    protected async Task AddStatusToType(Guid typeId, Guid statusId, CancellationToken ct)
    {
        var response = await Fixture.Client.PutAsync($"/work-item-types/{typeId}/statuses/{statusId}", null, ct);
        response.EnsureSuccessStatusCode();
        _cleanups.Push(c => Fixture.Client.DeleteAsync($"/work-item-types/{typeId}/statuses/{statusId}", c));
    }

    protected async Task<GetWorkItemTypeResponse> SetInitialStatus(Guid typeId, Guid statusId, CancellationToken ct)
    {
        var response = await Fixture.Client.PutAsJsonAsync($"/work-item-types/{typeId}/initial-status", new SetInitialStatusRequest(statusId), ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<GetWorkItemTypeResponse>(ApiFixture.JsonOptions, ct))!;
    }

    protected async Task<GetStatusTransitionResponse> CreateTransition(Guid typeId, Guid fromStatusId, Guid toStatusId, CancellationToken ct)
    {
        var response = await Fixture.Client.PostAsJsonAsync($"/work-item-types/{typeId}/transitions", new CreateStatusTransitionRequest(fromStatusId, toStatusId), ct);
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<GetStatusTransitionResponse>(ApiFixture.JsonOptions, ct))!;
        _cleanups.Push(c => Fixture.Client.DeleteAsync($"/work-item-types/{typeId}/transitions/{body.Id}", c));
        return body;
    }

    protected async Task<GetWorkItemFieldResponse> CreateField(Guid typeId, string key, WorkItemFieldDataType dataType, bool required, string? defaultValue, CancellationToken ct)
    {
        var response = await Fixture.Client.PostAsJsonAsync($"/work-item-types/{typeId}/fields", new CreateWorkItemFieldRequest(key, key, dataType, required, defaultValue), ct);
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<GetWorkItemFieldResponse>(ApiFixture.JsonOptions, ct))!;
        _cleanups.Push(c => Fixture.Client.DeleteAsync($"/work-item-types/{typeId}/fields/{body.Id}", c));
        return body;
    }

    protected async Task<GetFieldOptionResponse> CreateOption(Guid fieldId, string value, int rank, CancellationToken ct)
    {
        var response = await Fixture.Client.PostAsJsonAsync($"/fields/{fieldId}/options", new CreateFieldOptionRequest(value, value, rank), ct);
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<GetFieldOptionResponse>(ApiFixture.JsonOptions, ct))!;
        _cleanups.Push(c => Fixture.Client.DeleteAsync($"/fields/{fieldId}/options/{body.Id}", c));
        return body;
    }

    protected async Task<GetProjectResponse> CreateProject(string name, CancellationToken ct)
    {
        var response = await Fixture.Client.PostAsJsonAsync("/projects", new CreateProjectRequest(name), ct);
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<GetProjectResponse>(ApiFixture.JsonOptions, ct))!;
        _cleanups.Push(c => Fixture.Client.DeleteAsync($"/projects/{body.Id}", c));
        return body;
    }

    protected async Task OptInProjectToType(Guid projectId, Guid typeId, CancellationToken ct)
    {
        var response = await Fixture.Client.PutAsync($"/projects/{projectId}/work-item-types/{typeId}", null, ct);
        response.EnsureSuccessStatusCode();
        _cleanups.Push(c => Fixture.Client.DeleteAsync($"/projects/{projectId}/work-item-types/{typeId}", c));
    }
}
