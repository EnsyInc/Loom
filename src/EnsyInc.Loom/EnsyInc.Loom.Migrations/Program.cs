using EnsyInc.Loom.DataAccess;

using Microsoft.EntityFrameworkCore;

using NLog;
using NLog.Extensions.Logging;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddNLog();
});
builder.ConfigureServices((context, services) =>
{
    services.AddDataAccess(context.Configuration);
});

var app = builder.Build();

LogManager.Setup().LoadConfigurationFromFile("nlog.config");
var logger = LogManager.GetCurrentClassLogger();

try
{
    var dbContextFactory = app.Services.GetRequiredService<IDbContextFactory<LoomDbContext>>();
    await using var dbContext = dbContextFactory.CreateDbContext();

    logger.Info("Applying database migrations...");
    await dbContext.Database.MigrateAsync();
    logger.Info("Database migrations applied successfully.");
}
catch (Exception ex)
{
    logger.Fatal(ex, "Failed to apply database migrations.");
    LogManager.Flush();
    LogManager.Shutdown();
    Environment.Exit(1);
    throw;
}

LogManager.Flush();
LogManager.Shutdown();
