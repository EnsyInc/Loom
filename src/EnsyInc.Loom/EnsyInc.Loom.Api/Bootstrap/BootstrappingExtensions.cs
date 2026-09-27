using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Serialization;

using EnsyInc.Loom.Core.Config;
using EnsyInc.Loom.DataAccess;
using EnsyInc.Loom.Services;
using EnsyInc.Loom.Services.Abstractions;

using EnsyNet.Core.Configurations;

using Microsoft.AspNetCore.Authentication.JwtBearer;
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
                .AddApplicationServices()
                .AddEntraAuthentication(config);

        private IServiceCollection AddEntraAuthentication(IConfiguration config)
        {
            services.AddRequiredConfiguration<EntraConfig>(config, out var entraConfig);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt =>
                {
                    opt.Authority = entraConfig.GetAuthority();
                    opt.Audience = entraConfig.Audience;
                    // Only the Authority override (used by tests to point at a local mock issuer) runs over plain HTTP.
                    opt.RequireHttpsMetadata = string.IsNullOrWhiteSpace(entraConfig.Authority);
                    opt.MapInboundClaims = false;
                    opt.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = OnEntraTokenValidated,
                    };
                });

            return services.AddAuthorization();
        }

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
                .UseAuthentication()
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

    // Entra tokens carry no separate "login" event, so this is where a User row is provisioned on
    // first sign-in (or refreshed if the user's name/email changed in Entra since their last one).
    private static async Task OnEntraTokenValidated(TokenValidatedContext context)
    {
        var principal = context.Principal!;
        var entraObjectId = principal.FindFirst("oid")?.Value;

        if (string.IsNullOrWhiteSpace(entraObjectId))
        {
            context.Fail("The token is missing an 'oid' claim.");
            return;
        }

        var firstName = principal.FindFirst("given_name")?.Value ?? string.Empty;
        var lastName = principal.FindFirst("family_name")?.Value ?? string.Empty;
        var email = principal.FindFirst("preferred_username")?.Value ?? principal.FindFirst("email")?.Value ?? string.Empty;

        var usersService = context.HttpContext.RequestServices.GetRequiredService<IUsersService>();
        var result = await usersService.UpsertOnLogin(entraObjectId, firstName, lastName, email, context.HttpContext.RequestAborted);

        if (result.HasError)
        {
            context.Fail("Failed to provision the signed-in user.");
        }
    }
}
