using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SRGS.Infrastructure.Data;
using Xunit;

namespace SRGS.Infrastructure.Tests.Integration;

public sealed class AttachmentIntegrationTests : IClassFixture<SrgsApiFactory>
{
    private readonly SrgsApiFactory _factory;
    private readonly HttpClient _client;

    public AttachmentIntegrationTests(SrgsApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        _client.BaseAddress = new Uri("https://localhost");
    }

    [Fact]
    public async Task Upload_UsesAuthenticatedUserAndReturnsMetadata()
    {
        var accessToken = await LoginAndGetAccessTokenAsync();
        var requestId = await CreateRequestAsync(accessToken);
        var contents = "fake attachment contents"u8.ToArray();

        using var response = await UploadAsync(accessToken, requestId, "fake-notes.txt", contents, "application/octet-stream");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var metadata = await response.Content.ReadFromJsonAsync<AttachmentMetadata>();
        Assert.NotNull(metadata);
        Assert.Equal("fake-notes.txt", metadata.FileName);
        Assert.Equal("text/plain", metadata.ContentType);
        Assert.Equal(contents.Length, metadata.Size);

        using var scope = _factory.CreateDatabaseScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userId = await context.Users
            .Where(user => user.Username == SrgsApiFactory.TestUsername)
            .Select(user => user.Id)
            .SingleAsync();
        var attachment = await context.Attachments
            .SingleAsync(attachment => attachment.FileName == "fake-notes.txt");

        Assert.Equal(userId, attachment.UploadedByUserId);
        Assert.Equal(contents, attachment.FileData);
    }

    [Fact]
    public async Task Upload_WithoutToken_Returns401()
    {
        var requestId = await CreateRequestAsync(await LoginAndGetAccessTokenAsync());

        using var response = await UploadAsync(null, requestId, "anonymous.txt", "fake"u8.ToArray(), "text/plain");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Upload_OverSizeLimit_Returns400()
    {
        var accessToken = await LoginAndGetAccessTokenAsync();
        var requestId = await CreateRequestAsync(accessToken);
        var contents = new byte[5_242_881];

        using var response = await UploadAsync(accessToken, requestId, "too-large.txt", contents, "text/plain");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_WithDisallowedExtension_Returns400()
    {
        var accessToken = await LoginAndGetAccessTokenAsync();
        var requestId = await CreateRequestAsync(accessToken);

        using var response = await UploadAsync(accessToken, requestId, "fake.exe", "fake executable"u8.ToArray(), "application/octet-stream");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task List_ReturnsMetadataWithoutFileBytes()
    {
        var accessToken = await LoginAndGetAccessTokenAsync();
        var requestId = await CreateRequestAsync(accessToken);
        var contents = "metadata-only fake contents"u8.ToArray();
        using var uploadResponse = await UploadAsync(accessToken, requestId, "metadata.txt", contents, "text/plain");
        uploadResponse.EnsureSuccessStatusCode();

        using var response = await SendAsync(HttpMethod.Get, $"/api/requests/{requestId}/attachments", accessToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("fileData", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("metadata.txt", json, StringComparison.Ordinal);
        var metadata = JsonSerializer.Deserialize<List<AttachmentMetadata>>(
            json,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.NotNull(metadata);
        Assert.Contains(metadata, item => item.FileName == "metadata.txt" && item.Size == contents.Length);
    }

    [Fact]
    public async Task Download_ReturnsExactBytesAndStoredMetadata()
    {
        var accessToken = await LoginAndGetAccessTokenAsync();
        var requestId = await CreateRequestAsync(accessToken);
        var contents = "download fake contents"u8.ToArray();
        using var uploadResponse = await UploadAsync(accessToken, requestId, "download.txt", contents, "image/png");
        var metadata = await uploadResponse.Content.ReadFromJsonAsync<AttachmentMetadata>();

        using var response = await SendAsync(
            HttpMethod.Get,
            $"/api/requests/{requestId}/attachments/{metadata!.AttachmentId}",
            accessToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("download.txt", response.Content.Headers.ContentDisposition?.FileName?.Trim('"'));
        Assert.Equal(contents, await response.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task Download_WithMismatchedRequestId_Returns404()
    {
        var accessToken = await LoginAndGetAccessTokenAsync();
        var requestId = await CreateRequestAsync(accessToken);
        var otherRequestId = await CreateRequestAsync(accessToken);
        using var uploadResponse = await UploadAsync(accessToken, requestId, "mismatch.txt", "fake"u8.ToArray(), "text/plain");
        var metadata = await uploadResponse.Content.ReadFromJsonAsync<AttachmentMetadata>();

        using var response = await SendAsync(
            HttpMethod.Get,
            $"/api/requests/{otherRequestId}/attachments/{metadata!.AttachmentId}",
            accessToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_RemovesAttachmentAndSecondDeleteReturns404()
    {
        var accessToken = await LoginAndGetAccessTokenAsync();
        var requestId = await CreateRequestAsync(accessToken);
        using var uploadResponse = await UploadAsync(accessToken, requestId, "delete.txt", "fake"u8.ToArray(), "text/plain");
        var metadata = await uploadResponse.Content.ReadFromJsonAsync<AttachmentMetadata>();
        var path = $"/api/requests/{requestId}/attachments/{metadata!.AttachmentId}";

        using var deleteResponse = await SendAsync(HttpMethod.Delete, path, accessToken);
        using var secondDeleteResponse = await SendAsync(HttpMethod.Delete, path, accessToken);

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, secondDeleteResponse.StatusCode);

        using var scope = _factory.CreateDatabaseScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.False(await context.Attachments.AnyAsync(attachment => attachment.Id == metadata.AttachmentId));
    }

    private async Task<int> CreateRequestAsync(string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/requests")
        {
            Content = JsonContent.Create(new
            {
                title = $"Attachment request {Guid.NewGuid():N}",
                description = "Created by the attachment integration test.",
                requestTypeId = _factory.RequestTypeId,
                impactedModuleTypeId = _factory.ModuleTypeId,
                currentBehavior = (string?)null,
                expectedBehavior = (string?)null,
                businessJustification = "Attachment integration test justification.",
                priority = 0
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("requestId").GetInt32();
    }

    private async Task<HttpResponseMessage> UploadAsync(
        string? accessToken,
        int requestId,
        string fileName,
        byte[] contents,
        string contentType)
    {
        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(contents);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(fileContent, "file", fileName);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/requests/{requestId}/attachments")
        {
            Content = form
        };

        if (accessToken is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return await _client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, string accessToken)
    {
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return await _client.SendAsync(request);
    }

    private async Task<string> LoginAndGetAccessTokenAsync()
    {
        using var response = await _client.PostAsJsonAsync(
            "/identity/login",
            new { username = SrgsApiFactory.TestUsername, password = SrgsApiFactory.TestPassword });
        response.EnsureSuccessStatusCode();
        var tokens = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return tokens?.AccessToken ?? throw new InvalidOperationException("The test login did not return an access token.");
    }

    private sealed record TokenResponse(string AccessToken, string RefreshToken);

    private sealed class AttachmentMetadata
    {
        public int AttachmentId { get; init; }
        public string FileName { get; init; } = string.Empty;
        public string ContentType { get; init; } = string.Empty;
        public long Size { get; init; }
    }
}
