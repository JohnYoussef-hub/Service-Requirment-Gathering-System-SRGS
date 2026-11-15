using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SRGS.Domain.Requests.Enums;
using SRGS.Domain.Requests.History;
using SRGS.Infrastructure.Data;
using Xunit;

namespace SRGS.Infrastructure.Tests.Integration;

public sealed class RequestWorkflowIntegrationTests : IClassFixture<SrgsApiFactory>
{
    private static readonly SemaphoreSlim TestUserLock = new(1, 1);
    private readonly SrgsApiFactory _factory;
    private readonly HttpClient _client;

    public RequestWorkflowIntegrationTests(SrgsApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        _client.BaseAddress = new Uri("https://localhost");
    }

    [Fact]
    public async Task FullHappyPathFromSubmittedToClosedPersistsStatusHistory()
    {
        var userId = await GetTestUserIdAsync();
        var requestId = await CreateRequestAsync();

        await TransitionAsync(requestId, RequestStatus.UnderReview, "BA");
        await TransitionAsync(requestId, RequestStatus.Analysis, "BA");
        await TransitionAsync(requestId, RequestStatus.ManagementApproval, "BA");
        await DecideApprovalAsync(requestId, "Business Approver", ApprovalDecision.Approved, "Managerial");
        await DecideApprovalAsync(requestId, "Business Approver", ApprovalDecision.Approved, "Stakeholder");
        await AssignDeveloperAsync(requestId, userId, "Business Approver");
        await TransitionAsync(requestId, RequestStatus.InDevelopment, "Developer");
        await TransitionAsync(requestId, RequestStatus.InternalTesting, "Developer");
        await TransitionAsync(requestId, RequestStatus.UAT, "Developer");
        await TransitionAsync(requestId, RequestStatus.Deployed, "Developer");
        await TransitionAsync(requestId, RequestStatus.Closed, "BA");

        using var scope = _factory.CreateDatabaseScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var request = await context.Requests.SingleAsync(item => item.Id == requestId);
        var history = await context.ChangeHistories
            .Where(item => item.RequestId == requestId && item.OldStatus != null)
            .OrderBy(item => item.Id)
            .ToListAsync();

        Assert.Equal(RequestStatus.Closed, request.Status);
        Assert.Equal(10, history.Count);
        Assert.Equal(RequestStatus.Submitted, history[0].OldStatus);
        Assert.Equal(RequestStatus.Closed, history[^1].NewStatus);
        Assert.All(history, entry => Assert.False(string.IsNullOrWhiteSpace(entry.Comment)));
    }

    [Fact]
    public async Task IllegalJumpReturnsConflict()
    {
        var userId = await GetTestUserIdAsync();
        var requestId = await CreateRequestAsync();

        using var response = await SendAuthorizedAsync(
            HttpMethod.Post,
            $"/api/requests/{requestId}/transition",
            new { targetStatus = (int)RequestStatus.Analysis },
            "BA",
            userId);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task WrongRoleReturnsForbidden()
    {
        var userId = await GetTestUserIdAsync();
        var requestId = await CreateRequestAsync();

        using var response = await SendAuthorizedAsync(
            HttpMethod.Post,
            $"/api/requests/{requestId}/transition",
            new { targetStatus = (int)RequestStatus.UnderReview },
            "Requester",
            userId);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task TransitionWithoutTokenReturnsUnauthorized()
    {
        using var response = await _client.PostAsJsonAsync(
            "/api/requests/999999/transition",
            new { targetStatus = (int)RequestStatus.UnderReview });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<int> CreateRequestAsync()
    {
        using var response = await SendAuthorizedAsync(
            HttpMethod.Post,
            "/api/requests",
            new
            {
                title = $"Workflow request {Guid.NewGuid():N}",
                description = "Workflow integration test",
                requestTypeId = _factory.RequestTypeId,
                impactedModuleTypeId = _factory.ModuleTypeId,
                businessJustification = "Validate workflow"
            },
            "Requester",
            await GetTestUserIdAsync());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<RequestResponse>();
        Assert.NotNull(body);
        return body.RequestId;
    }

    private async Task TransitionAsync(int requestId, RequestStatus status, string role)
    {
        using var response = await SendAuthorizedAsync(
            HttpMethod.Post,
            $"/api/requests/{requestId}/transition",
            new { targetStatus = (int)status, note = $"Transition to {status}" },
            role,
            await GetTestUserIdAsync());

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task AssignDeveloperAsync(int requestId, int developerId, string role)
    {
        using var response = await SendAuthorizedAsync(
            HttpMethod.Post,
            $"/api/requests/{requestId}/assign-developer",
            new { developerId },
            role,
            await GetTestUserIdAsync());

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task DecideApprovalAsync(
        int requestId,
        string role,
        ApprovalDecision decision,
        string approvalType)
    {
        using var response = await SendAuthorizedAsync(
            HttpMethod.Post,
            $"/api/requests/{requestId}/approval",
            new
            {
                approvalType = approvalType == "Managerial" ? 1 : 0,
                decision = decision == ApprovalDecision.Approved ? 1 : 2,
                note = "Approval decision"
            },
            role,
            await GetTestUserIdAsync());

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<HttpResponseMessage> SendAuthorizedAsync(
        HttpMethod method,
        string uri,
        object body,
        string role,
        int userId)
    {
        using var request = new HttpRequestMessage(method, uri)
        {
            Content = JsonContent.Create(body)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            SrgsApiFactory.CreateAccessToken(userId, role));

        return await _client.SendAsync(request);
    }

    private async Task<int> GetTestUserIdAsync()
    {
        await TestUserLock.WaitAsync();
        try
        {
            using var scope = _factory.CreateDatabaseScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userId = await context.Users
                .Where(user => user.Username == SrgsApiFactory.TestUsername)
                .Select(user => user.Id)
                .SingleOrDefaultAsync();

            if (userId != 0)
            {
                return userId;
            }

            using var loginResponse = await _client.PostAsJsonAsync(
                "/identity/login",
                new
                {
                    username = SrgsApiFactory.TestUsername,
                    password = SrgsApiFactory.TestPassword
                });
            loginResponse.EnsureSuccessStatusCode();

            return await context.Users
                .Where(user => user.Username == SrgsApiFactory.TestUsername)
                .Select(user => user.Id)
                .SingleAsync();
        }
        finally
        {
            TestUserLock.Release();
        }
    }

    private sealed record RequestResponse(int RequestId);

    private enum ApprovalDecision
    {
        Approved,
        Rejected
    }
}
