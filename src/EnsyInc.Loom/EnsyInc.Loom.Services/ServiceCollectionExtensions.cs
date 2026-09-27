using EnsyInc.Loom.Services.Abstractions;
using EnsyInc.Loom.Services.Implementations;

using Microsoft.Extensions.DependencyInjection;

namespace EnsyInc.Loom.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        => services
            .AddScoped<IProjectsService, ProjectsService>()
            .AddScoped<ITplWorkItemStatusesService, TplWorkItemStatusesService>()
            .AddScoped<ITplWorkItemsService, TplWorkItemsService>()
            .AddScoped<IWorkItemTypeStatusesService, WorkItemTypeStatusesService>()
            .AddScoped<IStatusTransitionsService, StatusTransitionsService>()
            .AddScoped<ITplWorkItemFieldsService, TplWorkItemFieldsService>()
            .AddScoped<ITplWorkItemFieldOptionsService, TplWorkItemFieldOptionsService>()
            .AddScoped<IProjectWorkItemTypesService, ProjectWorkItemTypesService>();
}
