using System.Net;
using System.Net.Http.Headers;

using EnsyInc.Loom.ServiceTests.Fixtures;

namespace EnsyInc.Loom.ServiceTests.Auth;

/// <summary>Shared "this endpoint requires auth" assertion, reused by every endpoint's test class.</summary>
public static class AuthAssertions
{
    /// <summary>
    /// Asserts that <paramref name="relativeUrl"/> rejects both a missing and an invalid bearer token
    /// with 401. Authentication runs before model binding, so no request body is needed even for
    /// POST/PUT endpoints — the request never reaches the point where a body would matter.
    /// </summary>
    public static async Task AssertRequiresAuthentication(ApiFixture fixture, HttpMethod method, string relativeUrl, CancellationToken ct)
    {
        using var noTokenRequest = new HttpRequestMessage(method, relativeUrl);
        var noTokenResponse = await fixture.UnauthenticatedClient.SendAsync(noTokenRequest, ct);
        Assert.Equal(HttpStatusCode.Unauthorized, noTokenResponse.StatusCode);

        using var badTokenRequest = new HttpRequestMessage(method, relativeUrl);
        badTokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "this-is-not-a-valid-token");
        var badTokenResponse = await fixture.Client.SendAsync(badTokenRequest, ct);
        Assert.Equal(HttpStatusCode.Unauthorized, badTokenResponse.StatusCode);
    }
}
