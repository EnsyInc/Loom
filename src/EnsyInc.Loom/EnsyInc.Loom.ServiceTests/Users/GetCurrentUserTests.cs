using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Fixtures;
using EnsyInc.Loom.ServiceTests.Models;

namespace EnsyInc.Loom.ServiceTests.Users;

[Collection(ApiCollectionDefinition.Name)]
public sealed class GetCurrentUserTests(ApiFixture fixture)
{
    [Fact]
    public async Task GetCurrentUser_ValidToken_ReturnsProvisionedUser()
    {
        var ct = TestContext.Current.CancellationToken;
        var entraObjectId = Guid.NewGuid().ToString();
        var firstName = $"First-{Guid.NewGuid()}";
        var lastName = $"Last-{Guid.NewGuid()}";
        var email = $"{Guid.NewGuid()}@example.com";
        var token = fixture.MockEntraIssuer.MintToken(entraObjectId, firstName, lastName, email);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/users/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await fixture.Client.SendAsync(request, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<GetUserResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(firstName, user!.FirstName);
        Assert.Equal(lastName, user.LastName);
        Assert.Equal(email, user.Email);
    }

    [Fact]
    public async Task GetCurrentUser_NoToken_ReturnsUnauthorized()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await fixture.Client.GetAsync("/users/me", ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
