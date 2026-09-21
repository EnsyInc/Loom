using EnsyInc.Loom.Core.Config;
using EnsyInc.Loom.DataAccess.Abstractions;
using EnsyInc.Loom.DataAccess.Implementations;

using EnsyNet.Core.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnsyInc.Loom.DataAccess;

public static class ServiceCollectionsExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration config)
    {
        services.AddRequiredConfiguration<DbConfig>(config, out var dbConfig);
        var dbContextOptions = GetDbContextOptions(dbConfig);
        services.AddSingleton(dbContextOptions);

        services.AddDbContextFactory<LoomDbContext>();

        return services.AddRepos();
    }

    private static IServiceCollection AddRepos(this IServiceCollection services)
        => services.AddScoped<IProjectRepo, ProjectRepo>();

    private static DbContextOptions<LoomDbContext> GetDbContextOptions(DbConfig dbConfig)
        => new DbContextOptionsBuilder<LoomDbContext>()
            .UseSqlServer(dbConfig.ConnectionString, opts =>
            {
                opts.MigrationsAssembly("EnsyInc.Loom.Migrations")
                    .EnableRetryOnFailure();
            }).Options;
}
