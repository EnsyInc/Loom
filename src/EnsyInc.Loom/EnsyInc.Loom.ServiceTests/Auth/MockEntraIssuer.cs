using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

using Microsoft.IdentityModel.Tokens;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace EnsyInc.Loom.ServiceTests.Auth;

/// <summary>
/// A throwaway OIDC issuer (discovery + JWKS), stubbed with WireMock.Net, that the real Api is pointed
/// at via its <c>Entra__Authority</c> override, so ServiceTests can authenticate without a real Entra
/// tenant. Lives entirely in the test project — see EnsyInc.Loom.Api.Bootstrap.BootstrappingExtensions
/// for the generic, non-test-specific <c>Authority</c> override this relies on.
/// </summary>
public sealed class MockEntraIssuer : IDisposable
{
    // Fixed and well-known: the real Api reads Entra__Authority once at startup, before this process
    // (and this issuer) exists, so both sides must agree on where it will be ahead of time.
    public const int Port = 5998;
    public const string RoutePrefix = "testing-auth";
    public const string Audience = "loom-service-tests";

    // Fixed test-only key (non-secret, never used for anything but this mock issuer) instead of a
    // freshly generated one each run: the real Api is typically a long-lived process across several
    // local test runs and caches the JWKS it fetches, so a random key here would go stale against
    // that cache the moment this process restarts with a different one.
    private const string KeyId = "loom-service-tests-signing-key";
    private static readonly RSAParameters SigningKeyParameters = new()
    {
        Modulus = Convert.FromBase64String("4efKY8fHHmAF1U3mWEx4gXy8eXR4cG3bNqtutLARCYaR0Le1O6wqt09ziKNcUqMdkJT571uOeXhFa8MAoRf2aJFIqfctJBQ4bFXAO4foccSvBHAQNzN5LtCJ49c/ADE59p+bFd483CuRkXXcX5zT+VcHz6XBtKoBGqwoACzRkbl2ky+TLW9022VVAGlU1XIZatyIYsZ3UucxX7+64YRZAO90PHR9A1IMJCuidLDn3jVN9OOF4/2QFNCbuqj3BGzeqUG72cy2z+kTIs4e15v9Rm6bMlntXuxvg4GbHDCQlZZbd3Ivm7hnPM9ERbMPQKJsV3KVNdmGnp5kzWf05vLOnQ=="),
        Exponent = Convert.FromBase64String("AQAB"),
        D = Convert.FromBase64String("A8HZKeOuuiCauDt9c0aCUpUChXqcbjJFjRk41CSBhAkK78++YemxJC9cGo+s/2rbGQaBSavaKMcJUIuDvSoq3P5dmQYVaB41+gdLFTJjeVDuIELE4kYm49JwGKwBzj0DW8i3yZtSo87Nsv+k1KpFFEaQSLHXEOwq7IikeaxFKAlHoCosVGut8uHRac34CX142JNws1MA9svQXBkuHXk3TwZ5QVzXxMKNirhJt08UP6lKDEGfissqJywkGBiB7+V1co5KbmcMlro726tk7rmeWLtg1YpWlww4i4FXqhZp1HpzBNpBUzTtlBf008y9kPg/UhHt7C/zxVLCM8AR93lj+Q=="),
        P = Convert.FromBase64String("+hOvPF8WJm7JlZvn0Tzklf3rc0PTmHrviexGlGEXpUZnn3KR4smOqxkWLZqT0d+cNBCmyzO7A85lQBozbZVZcsENPnoIZNtOusKFRm+UzWmQrxuTebUlk8gxKmmHRjnF3vjP+D7WleVEDaSRgrpcB25Crhiefl9vxviGGlKZ9W8="),
        Q = Convert.FromBase64String("50GLfxAbr4Z/fqjC182/Oargz+RNdawZrCs+YnkQ7An3Fw7pYfOy3ikvqJ3NVixFFrXPzOjuPrD6U5X73tQ1ZVPVX55cv3Qswwa+IphHwd1mtstYCS4MeSdgmxtkQqI1IB9zM7s8wopiIYyQjYgymPnNu2cBvFJRBJTvitVR7rM="),
        DP = Convert.FromBase64String("cggf0+uX8hZ6nVnmLycRJMQQZRL8nX8RU1cGKArN+XNNNQvNMhiukZ5y2oCt/vl0BNnC9M217VpkGQLZiJKdoxIcs+x5f2PxVn/0vdWLiM8mRnLQoKLVa7nRkYFlScR2UeIrEwu0Vc2hZocwQugvpsPEbBaVzxI9qgERRVd9FMk="),
        DQ = Convert.FromBase64String("BGkpofjKrmRgVigd30PbW9w4gX6XQ6FOtAv3GqW4fugJwHTWWrntNXOPpyWrXvNlNfOtzPi8YpG5lJKNxUhC5HXe4f4BQt2SZP3h7oL9C2OPq/jg6vvn3P+1RgAVv3ecj8pJxjjSBdvh10X5E9n/LQ/GrsZeMBtZ1D6K+xjqgSU="),
        InverseQ = Convert.FromBase64String("rKVXE0Lp1/WwCMSfwtacp+s53aUzabkltfDP2h3EeRZ5ql+NW70y5PjuNyC+LOfdvRhPVsW+JTPsrgW258hKmrGXcMCnxv4katK1wZ4xbC5se44/5HHatmxJPPDRr/2RJYh8G6EFkhEiTdcWdcwir1D+DmcEiphNSDqILkj5IQ4="),
    };

    private readonly RsaSecurityKey _signingKey = CreateSigningKey();
    private WireMockServer? _server;

    private static RsaSecurityKey CreateSigningKey()
    {
        var rsa = RSA.Create();
        rsa.ImportParameters(SigningKeyParameters);
        return new RsaSecurityKey(rsa) { KeyId = KeyId };
    }

    public string Authority { get; } = $"http://localhost:{Port}/{RoutePrefix}";

    public void Start()
    {
        _server = WireMockServer.Start(Port);

        _server.Given(Request.Create().WithPath($"/{RoutePrefix}/.well-known/openid-configuration").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new
                {
                    issuer = Authority,
                    jwks_uri = $"{Authority}/jwks",
                    id_token_signing_alg_values_supported = new[] { SecurityAlgorithms.RsaSha256 },
                }));

        var rsaParameters = _signingKey.Rsa.ExportParameters(false);
        var jwk = new
        {
            kty = "RSA",
            use = "sig",
            kid = _signingKey.KeyId,
            n = Base64UrlEncoder.Encode(rsaParameters.Modulus),
            e = Base64UrlEncoder.Encode(rsaParameters.Exponent),
            alg = SecurityAlgorithms.RsaSha256,
        };

        _server.Given(Request.Create().WithPath($"/{RoutePrefix}/jwks").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new { keys = new[] { jwk } }));
    }

    public string MintToken(string entraObjectId, string firstName, string lastName, string email)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = new JwtSecurityToken(
            issuer: Authority,
            audience: Audience,
            claims:
            [
                new Claim("oid", entraObjectId),
                new Claim("given_name", firstName),
                new Claim("family_name", lastName),
                new Claim("preferred_username", email),
            ],
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.RsaSha256));

        return handler.WriteToken(token);
    }

    public void Dispose()
        => _server?.Stop();
}
