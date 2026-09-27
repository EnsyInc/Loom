using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using EnsyInc.Loom.ServiceTests.Auth;
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
    public async Task GetCurrentUser_ReturningUserWithChangedProfile_ReflectsUpdatedProfile()
    {
        var ct = TestContext.Current.CancellationToken;
        var entraObjectId = Guid.NewGuid().ToString();

        var firstToken = fixture.MockEntraIssuer.MintToken(entraObjectId, "Original-First", "Original-Last", "original@example.com");
        using var firstRequest = new HttpRequestMessage(HttpMethod.Get, "/users/me");
        firstRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", firstToken);
        var firstResponse = await fixture.Client.SendAsync(firstRequest, ct);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        var firstUser = await firstResponse.Content.ReadFromJsonAsync<GetUserResponse>(ApiFixture.JsonOptions, ct);

        var newEmail = $"{Guid.NewGuid()}@example.com";
        var secondToken = fixture.MockEntraIssuer.MintToken(entraObjectId, "Updated-First", "Updated-Last", newEmail);
        using var secondRequest = new HttpRequestMessage(HttpMethod.Get, "/users/me");
        secondRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secondToken);
        var secondResponse = await fixture.Client.SendAsync(secondRequest, ct);

        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        var secondUser = await secondResponse.Content.ReadFromJsonAsync<GetUserResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(firstUser!.Id, secondUser!.Id);
        Assert.Equal("Updated-First", secondUser.FirstName);
        Assert.Equal("Updated-Last", secondUser.LastName);
        Assert.Equal(newEmail, secondUser.Email);
    }

    [Fact]
    public async Task GetCurrentUser_NoTokenOrBadToken_ReturnsUnauthorized()
    {
        var ct = TestContext.Current.CancellationToken;
        await AuthAssertions.AssertRequiresAuthentication(fixture, HttpMethod.Get, "/users/me", ct);
    }
}
