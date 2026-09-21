using EnsyInc.Loom.Services.Abstractions;
using EnsyInc.Loom.Services.Implementations;

using Microsoft.Extensions.DependencyInjection;

namespace EnsyInc.Loom.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        => services.AddScoped<IProjectsService, ProjectsService>();
}
