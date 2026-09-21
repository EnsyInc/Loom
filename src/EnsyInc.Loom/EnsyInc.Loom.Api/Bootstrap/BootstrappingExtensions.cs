using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Serialization;

using EnsyInc.Loom.DataAccess;
using EnsyInc.Loom.Services;

using Microsoft.OpenApi;

using NLog;
using NLog.Extensions.Logging;

namespace EnsyInc.Loom.Api.Bootstrap;

[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "The compiler emits a member literally named 'extension' for each C# extension block; there's no way to rename a compiler-synthesized symbol.")]
internal static class BootstrappingExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder InitializeApplication()
        {
            builder.Configuration.InitializeConfiguration(builder.Environment.EnvironmentName);
            builder.Logging.ConfigureLogging();
            builder.Services.AddServices(builder.Configuration);

            return builder;
        }
    }

    extension(ConfigurationManager config)
    {
        private void InitializeConfiguration(string envName)
        {
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
            config.AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: false);
            config.AddEnvironmentVariables();
            var secretsFolder = config["SECRETS_FOLDER"];
            if (!string.IsNullOrWhiteSpace(secretsFolder))
            {
                config.AddKeyPerFile(secretsFolder, optional: true, reloadOnChange: false);
            }
        }
    }

    extension(ILoggingBuilder builder)
    {
        private void ConfigureLogging()
        {
            builder.ClearProviders();
            builder.AddNLog();
            LogManager.Setup().LoadConfigurationFromFile("nlog.config");
        }
    }

    extension(IServiceCollection services)
    {
        private void AddServices(IConfiguration config)
            => services.AddDefaultServices()
                .AddDataAccess(config)
                .AddApplicationServices();

        private IServiceCollection AddDefaultServices()
        {
            services.AddControllers()
                .AddJsonOptions(opt => opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            services.AddOpenApi()
                .AddEndpointsApiExplorer()
                .AddSwaggerGen(opt =>
                {
                    opt.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "EnsyInc.Loom API",
                        Version = "v1",
                        Description = "Schema-driven work tracker API.",
                    });

                    var xmlFilename = $"{Assembly.GetEntryAssembly()!.GetName().Name}.xml";
                    opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
                });
            return services;
        }
    }

    extension(WebApplication app)
    {
        public WebApplication ConfigureApplication()
            => app.AddDefaultAppPipeline();

        private WebApplication AddDefaultAppPipeline()
        {
            app.UseExceptionHandler();
            app.MapOpenApi();
            app.UseSwagger()
                .UseSwaggerUI();
            app.UseHttpsRedirection()
                .UseAuthorization();
            app.MapControllers();

            return app;
        }

        public void RunApplication()
        {
            try
            {
                app.Run();
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Fatal(ex, "An error occurred in the application");
                LogManager.Flush();
                LogManager.Shutdown();
                Environment.Exit(1);
                throw;
            }

            LogManager.Flush();
            LogManager.Shutdown();
        }
    }
}
