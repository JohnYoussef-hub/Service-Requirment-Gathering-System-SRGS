using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SRGS.Application.Features.Identity;
using SRGS.Infrastructure.Data;
using Xunit;

namespace SRGS.Infrastructure.Tests.Integration;

public sealed class AuthenticationFlowTests : IClassFixture<SrgsApiFactory>
{
    private readonly SrgsApiFactory _factory;
    private readonly HttpClient _client;

    public AuthenticationFlowTests(SrgsApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        _client.BaseAddress = new Uri("https://localhost");
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200AndTokens()
    {
        using var response = await LoginAsync(SrgsApiFactory.TestUsername, SrgsApiFactory.TestPassword);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

        Assert.NotNull(tokenResponse);
        Assert.False(string.IsNullOrWhiteSpace(tokenResponse.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(tokenResponse.RefreshToken));
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401WithGenericError()
    {
        using var wrongUsernameResponse = await LoginAsync("unknown.user", SrgsApiFactory.TestPassword);
        using var wrongPasswordResponse = await LoginAsync(SrgsApiFactory.TestUsername, "wrong-password");

        Assert.Equal(HttpStatusCode.Unauthorized, wrongUsernameResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, wrongPasswordResponse.StatusCode);

        var wrongUsernameShape = await ReadErrorShapeAsync(wrongUsernameResponse);
        var wrongPasswordShape = await ReadErrorShapeAsync(wrongPasswordResponse);

        Assert.Equal(wrongUsernameShape.Properties, wrongPasswordShape.Properties);
        Assert.Equal(wrongUsernameShape.Status, wrongPasswordShape.Status);
        Assert.Equal(wrongUsernameShape.Title, wrongPasswordShape.Title);
        Assert.Equal("The username or password is invalid.", wrongUsernameShape.Title);
    }

    [Fact]
    public async Task Refresh_RotatesTokenWithinTheSameFamily()
    {
        var initialTokens = await LoginAndGetTokensAsync();

        using var response = await RefreshAsync(initialTokens);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var rotatedTokens = await response.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(rotatedTokens);

        using var scope = _factory.CreateDatabaseScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var storedTokens = await context.RefreshTokens
            .Where(token => token.Token == HashRefreshToken(initialTokens.RefreshToken!) ||
                            token.Token == HashRefreshToken(rotatedTokens.RefreshToken!))
            .ToListAsync();

        Assert.Equal(2, storedTokens.Count);
        Assert.Equal(storedTokens[0].FamilyId, storedTokens[1].FamilyId);
        Assert.Contains(storedTokens, token => token.IsUsed && !token.IsRevoked);
        Assert.Contains(storedTokens, token => !token.IsUsed && !token.IsRevoked);
    }

    [Fact]
    public async Task Refresh_ReusingTokenRevokesTheEntireFamily()
    {
        var initialTokens = await LoginAndGetTokensAsync();
        using var firstRefreshResponse = await RefreshAsync(initialTokens);
        firstRefreshResponse.EnsureSuccessStatusCode();
        var rotatedTokens = await firstRefreshResponse.Content.ReadFromJsonAsync<TokenResponse>();

        using var reuseResponse = await RefreshAsync(initialTokens);

        Assert.Equal(HttpStatusCode.Unauthorized, reuseResponse.StatusCode);
        var error = await ReadErrorShapeAsync(reuseResponse);
        Assert.Equal("The refresh token is invalid.", error.Title);

        using var scope = _factory.CreateDatabaseScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var familyTokens = await context.RefreshTokens
            .Where(token => token.Token == HashRefreshToken(initialTokens.RefreshToken!) ||
                            token.Token == HashRefreshToken(rotatedTokens!.RefreshToken!))
            .ToListAsync();

        Assert.Equal(2, familyTokens.Count);
        Assert.All(familyTokens, token => Assert.True(token.IsRevoked));
    }

    [Fact]
    public async Task Refresh_WithTokenFromRevokedFamilyReturnsGenericInvalidError()
    {
        var initialTokens = await LoginAndGetTokensAsync();
        using var firstRefreshResponse = await RefreshAsync(initialTokens);
        firstRefreshResponse.EnsureSuccessStatusCode();
        var rotatedTokens = await firstRefreshResponse.Content.ReadFromJsonAsync<TokenResponse>();

        using var reuseResponse = await RefreshAsync(initialTokens);
        Assert.Equal(HttpStatusCode.Unauthorized, reuseResponse.StatusCode);

        using var revokedTokenResponse = await RefreshAsync(rotatedTokens!);

        Assert.Equal(HttpStatusCode.Unauthorized, revokedTokenResponse.StatusCode);
        var error = await ReadErrorShapeAsync(revokedTokenResponse);
        Assert.Equal("The refresh token is invalid.", error.Title);
    }

    [Fact]
    public async Task CreateRequest_WithoutAuthHeader_Returns401()
    {
        int initialRequestCount;
        using (var scope = _factory.CreateDatabaseScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            initialRequestCount = await context.Requests.CountAsync();
        }

        using var response = await _client.PostAsJsonAsync("/api/requests", CreateRequestPayload());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        using var verificationScope = _factory.CreateDatabaseScope();
        var verificationContext = verificationScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(initialRequestCount, await verificationContext.Requests.CountAsync());
    }

    [Fact]
    public async Task CreateRequest_WithValidToken_SetsRequestedByIdFromToken()
    {
        var accessToken = await LoginAndGetAccessTokenAsync();
        int userId;

        using (var scope = _factory.CreateDatabaseScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            userId = await context.Users
                .Where(user => user.Username == SrgsApiFactory.TestUsername)
                .Select(user => user.Id)
                .SingleAsync();
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/requests")
        {
            Content = JsonContent.Create(CreateRequestPayload())
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var verificationScope = _factory.CreateDatabaseScope();
        var verificationContext = verificationScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var createdRequest = await verificationContext.Requests
            .SingleAsync(entity => entity.Title == "Integration request");

        Assert.Equal(userId, createdRequest.RequestedById);
    }

    [Fact]
    public async Task CreateRequest_WithTamperedToken_Returns401()
    {
        var accessToken = await LoginAndGetAccessTokenAsync();
        var tokenParts = accessToken.Split('.');
        var signature = tokenParts[2].ToCharArray();
        signature[0] = signature[0] == 'A' ? 'B' : 'A';
        tokenParts[2] = new string(signature);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/requests")
        {
            Content = JsonContent.Create(CreateRequestPayload())
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", string.Join('.', tokenParts));

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateRequest_WithExpiredToken_Returns401()
    {
        await LoginAndGetAccessTokenAsync();
        int userId;

        using (var scope = _factory.CreateDatabaseScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            userId = await context.Users
                .Where(user => user.Username == SrgsApiFactory.TestUsername)
                .Select(user => user.Id)
                .SingleAsync();
        }

        var expiredToken = SrgsApiFactory.CreateExpiredToken(userId);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/requests")
        {
            Content = JsonContent.Create(CreateRequestPayload())
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<HttpResponseMessage> LoginAsync(string username, string password)
        => await _client.PostAsJsonAsync("/identity/login", new { username, password });

    private async Task<TokenResponse> LoginAndGetTokensAsync()
    {
        using var response = await LoginAsync(SrgsApiFactory.TestUsername, SrgsApiFactory.TestPassword);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TokenResponse>()
            ?? throw new InvalidOperationException("The test login did not return tokens.");
    }

    private async Task<HttpResponseMessage> RefreshAsync(TokenResponse tokens)
        => await _client.PostAsJsonAsync("/identity/refresh", new
        {
            refreshToken = tokens.RefreshToken,
            expiredAccessToken = tokens.AccessToken
        });

    private async Task<string> LoginAndGetAccessTokenAsync()
    {
        using var response = await LoginAsync(SrgsApiFactory.TestUsername, SrgsApiFactory.TestPassword);
        response.EnsureSuccessStatusCode();
        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return tokenResponse?.AccessToken ?? throw new InvalidOperationException("The test login did not return an access token.");
    }

    private object CreateRequestPayload()
        => new
        {
            title = "Integration request",
            description = "Created by the HTTP integration test.",
            requestTypeId = _factory.RequestTypeId,
            impactedModuleTypeId = _factory.ModuleTypeId,
            currentBehavior = (string?)null,
            expectedBehavior = (string?)null,
            businessJustification = "Integration test justification.",
            priority = 0
        };

    private static async Task<ErrorShape> ReadErrorShapeAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var root = document.RootElement;

        var properties = root.EnumerateObject()
            .Where(property => property.Name is not "traceId")
            .Select(property => property.Name)
            .OrderBy(name => name)
            .ToArray();

        return new ErrorShape(
            properties,
            root.GetProperty("status").GetInt32(),
            root.GetProperty("title").GetString());
    }

    private static string HashRefreshToken(string refreshToken)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

    private sealed record ErrorShape(string[] Properties, int Status, string? Title);
}
