using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Configuration;

namespace EnsyInc.Loom.ServiceTests.Fixtures;

public sealed class ApiFixture : IAsyncLifetime, IAsyncDisposable
{
    // Mirrors what ReadFromJsonAsync uses when no options are passed (case-insensitive, camelCase),
    // plus the enum-as-string converter the Api registers (see BootstrappingExtensions.AddDefaultServices)
    // since the default JsonSerializerOptions otherwise expects numeric enum values.
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public HttpClient Client { get; private set; } = null!;

    public ValueTask InitializeAsync()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var apiBaseUrl = config["ApiBaseUrl"]
            ?? throw new InvalidOperationException("ApiBaseUrl is not configured.");

        // The local dev HTTPS cert is self-signed; this is test-only and must never run against production.
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
        };

        Client = new HttpClient(handler) { BaseAddress = new Uri(apiBaseUrl) };

        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        Client.Dispose();
        return ValueTask.CompletedTask;
    }
}

[CollectionDefinition(Name)]
public sealed class ApiCollectionDefinition : ICollectionFixture<ApiFixture>
{
    public const string Name = "Api";
}
