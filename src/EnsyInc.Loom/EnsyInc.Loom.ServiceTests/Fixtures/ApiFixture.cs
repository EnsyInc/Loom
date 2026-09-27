using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

using EnsyInc.Loom.ServiceTests.Auth;

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

    /// <summary>Authenticated by default: every request gets a valid bearer token unless the request
    /// already sets its own <c>Authorization</c> header (e.g. to test a bad token).</summary>
    public HttpClient Client { get; private set; } = null!;

    /// <summary>No auth header at all. Used only by the "no token" 401 tests.</summary>
    public HttpClient UnauthenticatedClient { get; private set; } = null!;

    public MockEntraIssuer MockEntraIssuer { get; } = new();

    public ValueTask InitializeAsync()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var apiBaseUrl = config["ApiBaseUrl"]
            ?? throw new InvalidOperationException("ApiBaseUrl is not configured.");

        // Started before any test runs. The real Api (a separate, already-running process) is only
        // configured to trust this issuer once it actually needs to validate a token, so starting it
        // here — rather than exactly when a test needs it — is early enough.
        MockEntraIssuer.Start();
        var token = MockEntraIssuer.MintToken(
            entraObjectId: $"servicetests-{Guid.NewGuid()}",
            firstName: "ServiceTests",
            lastName: "User",
            email: "servicetests@example.com");

        // The local dev HTTPS cert is self-signed; this is test-only and must never run against production.
        Client = new HttpClient(new AuthTokenHandler(token) { InnerHandler = NewCertBypassHandler() }) { BaseAddress = new Uri(apiBaseUrl) };
        UnauthenticatedClient = new HttpClient(NewCertBypassHandler()) { BaseAddress = new Uri(apiBaseUrl) };

        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        Client.Dispose();
        UnauthenticatedClient.Dispose();
        MockEntraIssuer.Dispose();

        return ValueTask.CompletedTask;
    }

    private static HttpClientHandler NewCertBypassHandler()
        => new() { ServerCertificateCustomValidationCallback = (_, _, _, _) => true };

    private sealed class AuthTokenHandler(string token) : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Authorization ??= new AuthenticationHeaderValue("Bearer", token);
            return base.SendAsync(request, cancellationToken);
        }
    }
}

[CollectionDefinition(Name)]
public sealed class ApiCollectionDefinition : ICollectionFixture<ApiFixture>
{
    public const string Name = "Api";
}
