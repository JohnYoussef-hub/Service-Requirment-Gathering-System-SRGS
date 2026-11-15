using SRGS.Domain.Requests;
using SRGS.Domain.Requests.Enums;
using SRGS.Domain.Requests.Events;
using Xunit;

namespace SRGS.Domain.Tests;

public sealed class RequestWorkflowTests
{
    public static IEnumerable<object[]> AllowedTransitions =>
    [
        [new[] { RequestStatus.Submitted, RequestStatus.UnderReview }, "BA"],
        [new[] { RequestStatus.Submitted, RequestStatus.Cancelled }, "Requester"],
        [new[] { RequestStatus.Submitted, RequestStatus.UnderReview, RequestStatus.ClarificationRequired }, "BA"],
        [new[] { RequestStatus.Submitted, RequestStatus.UnderReview, RequestStatus.Analysis }, "BA"],
        [new[] { RequestStatus.Submitted, RequestStatus.UnderReview, RequestStatus.Rejected }, "BA"],
        [new[] { RequestStatus.Submitted, RequestStatus.UnderReview, RequestStatus.ClarificationRequired, RequestStatus.UnderReview }, "Requester"],
        [new[] { RequestStatus.Submitted, RequestStatus.UnderReview, RequestStatus.ClarificationRequired, RequestStatus.Cancelled }, "Requester"],
        [new[] { RequestStatus.Submitted, RequestStatus.UnderReview, RequestStatus.Analysis, RequestStatus.ManagementApproval }, "BA"],
        [new[] { RequestStatus.Submitted, RequestStatus.UnderReview, RequestStatus.Analysis, RequestStatus.ClarificationRequired }, "BA"],
        [new[] { RequestStatus.Submitted, RequestStatus.UnderReview, RequestStatus.Analysis, RequestStatus.Rejected }, "BA"],
        [new[] { RequestStatus.Submitted, RequestStatus.UnderReview, RequestStatus.Analysis, RequestStatus.ManagementApproval, RequestStatus.StakeholdersApproval }, "Business Approver"],
        [new[] { RequestStatus.Submitted, RequestStatus.UnderReview, RequestStatus.Analysis, RequestStatus.ManagementApproval, RequestStatus.ClarificationRequired }, "Business Approver"],
        [new[] { RequestStatus.Submitted, RequestStatus.UnderReview, RequestStatus.Analysis, RequestStatus.ManagementApproval, RequestStatus.Rejected }, "Business Approver"],
        [new[] { RequestStatus.Submitted, RequestStatus.UnderReview, RequestStatus.Analysis, RequestStatus.ManagementApproval, RequestStatus.StakeholdersApproval, RequestStatus.Assigned }, "Business Approver"],
        [new[] { RequestStatus.Submitted, RequestStatus.UnderReview, RequestStatus.Analysis, RequestStatus.ManagementApproval, RequestStatus.StakeholdersApproval, RequestStatus.ClarificationRequired }, "Business Approver"],
        [new[] { RequestStatus.Submitted, RequestStatus.UnderReview, RequestStatus.Analysis, RequestStatus.ManagementApproval, RequestStatus.StakeholdersApproval, RequestStatus.Rejected }, "Business Approver"],
        [new[] { RequestStatus.Submitted, RequestStatus.UnderReview, RequestStatus.Analysis, RequestStatus.ManagementApproval, RequestStatus.StakeholdersApproval, RequestStatus.Assigned, RequestStatus.InDevelopment }, "Developer"],
        [new[] { RequestStatus.Submitted, RequestStatus.UnderReview, RequestStatus.Analysis, RequestStatus.ManagementApproval, RequestStatus.StakeholdersApproval, RequestStatus.Assigned, RequestStatus.InDevelopment, RequestStatus.InternalTesting }, "Developer"],
        [new[] { RequestStatus.Submitted, RequestStatus.UnderReview, RequestStatus.Analysis, RequestStatus.ManagementApproval, RequestStatus.StakeholdersApproval, RequestStatus.Assigned, RequestStatus.InDevelopment, RequestStatus.InternalTesting, RequestStatus.UAT }, "Developer"],
        [new[] { RequestStatus.Submitted, RequestStatus.UnderReview, RequestStatus.Analysis, RequestStatus.ManagementApproval, RequestStatus.StakeholdersApproval, RequestStatus.Assigned, RequestStatus.InDevelopment, RequestStatus.InternalTesting, RequestStatus.InDevelopment }, "Developer"],
        [new[] { RequestStatus.Submitted, RequestStatus.UnderReview, RequestStatus.Analysis, RequestStatus.ManagementApproval, RequestStatus.StakeholdersApproval, RequestStatus.Assigned, RequestStatus.InDevelopment, RequestStatus.InternalTesting, RequestStatus.UAT, RequestStatus.Deployed }, "Developer"],
        [new[] { RequestStatus.Submitted, RequestStatus.UnderReview, RequestStatus.Analysis, RequestStatus.ManagementApproval, RequestStatus.StakeholdersApproval, RequestStatus.Assigned, RequestStatus.InDevelopment, RequestStatus.InternalTesting, RequestStatus.UAT, RequestStatus.InDevelopment }, "Requester"],
        [new[] { RequestStatus.Submitted, RequestStatus.UnderReview, RequestStatus.Analysis, RequestStatus.ManagementApproval, RequestStatus.StakeholdersApproval, RequestStatus.Assigned, RequestStatus.InDevelopment, RequestStatus.InternalTesting, RequestStatus.UAT, RequestStatus.Deployed, RequestStatus.Closed }, "BA"]
    ];

    [Theory]
    [MemberData(nameof(AllowedTransitions))]
    public void TransitionTo_allows_each_brd_transition(RequestStatus[] path, string role)
    {
        var request = CreateRequest();

        foreach (var target in path.Skip(1).SkipLast(1))
        {
            var result = request.TransitionTo(
                target,
                ["BA", "Requester", "Business Approver", "Developer"],
                1,
                "workflow test note");

            Assert.True(result.IsSuccess, result.TopError.Description);
        }

        var finalResult = request.TransitionTo(path[^1], [role], 1, "workflow test note");

        Assert.True(finalResult.IsSuccess, finalResult.TopError.Description);
        Assert.Equal(path[^1], request.Status);
    }

    [Fact]
    public void TransitionTo_raises_status_changed_event()
    {
        var request = CreateRequest();

        var result = request.TransitionTo(RequestStatus.UnderReview, ["BA"], 1);

        Assert.True(result.IsSuccess);
        var domainEvent = Assert.Single(request.DomainEvents.OfType<RequestStatusChanged>());
        Assert.Equal(RequestStatus.Submitted, domainEvent.OldStatus);
        Assert.Equal(RequestStatus.UnderReview, domainEvent.NewStatus);
    }

    [Fact]
    public void TransitionTo_rejects_illegal_jump()
    {
        var result = CreateRequest().TransitionTo(RequestStatus.Analysis, ["BA"], 1);

        Assert.Equal("Request.InvalidStatusTransition", result.TopError.Code);
    }

    [Fact]
    public void TransitionTo_rejects_wrong_role()
    {
        var result = CreateRequest().TransitionTo(RequestStatus.UnderReview, ["Developer"], 1);

        Assert.Equal("Request.StatusTransition.Forbidden", result.TopError.Code);
    }

    [Theory]
    [InlineData(RequestStatus.Cancelled)]
    [InlineData(RequestStatus.Rejected)]
    [InlineData(RequestStatus.ClarificationRequired)]
    public void TransitionTo_requires_reason_for_relevant_statuses(RequestStatus target)
    {
        var request = CreateRequest();
        var path = target switch
        {
            RequestStatus.Cancelled => new[] { RequestStatus.Submitted },
            RequestStatus.Rejected or RequestStatus.ClarificationRequired =>
                new[] { RequestStatus.Submitted, RequestStatus.UnderReview },
            _ => throw new ArgumentOutOfRangeException(nameof(target))
        };

        foreach (var setupTarget in path.Skip(1))
        {
            var setupResult = request.TransitionTo(
                setupTarget,
                ["BA", "Requester", "Business Approver", "Developer"],
                1,
                "setup note");

            Assert.True(setupResult.IsSuccess, setupResult.TopError.Description);
        }

        var role = target == RequestStatus.Cancelled ? "Requester" : "BA";
        var result = request.TransitionTo(target, [role], 1);

        Assert.Equal("Request.TransitionReason.Required", result.TopError.Code);
    }

    [Theory]
    [InlineData(RequestStatus.Cancelled, "Requester")]
    [InlineData(RequestStatus.Rejected, "BA")]
    [InlineData(RequestStatus.Closed, "BA")]
    public void TransitionTo_does_not_allow_transitions_out_of_terminal_states(
        RequestStatus terminalStatus,
        string role)
    {
        var request = CreateRequest();
        RequestStatus[] terminalPath = terminalStatus switch
        {
            RequestStatus.Cancelled => [RequestStatus.Submitted, RequestStatus.Cancelled],
            RequestStatus.Rejected =>
                [RequestStatus.Submitted, RequestStatus.UnderReview, RequestStatus.Analysis, RequestStatus.Rejected],
            RequestStatus.Closed =>
                [RequestStatus.Submitted, RequestStatus.UnderReview, RequestStatus.Analysis,
                    RequestStatus.ManagementApproval, RequestStatus.StakeholdersApproval, RequestStatus.Assigned,
                    RequestStatus.InDevelopment, RequestStatus.InternalTesting, RequestStatus.UAT,
                    RequestStatus.Deployed, RequestStatus.Closed],
            _ => throw new ArgumentOutOfRangeException(nameof(terminalStatus))
        };

        foreach (var target in terminalPath.Skip(1))
        {
            var setupResult = request.TransitionTo(
                target,
                ["BA", "Requester", "Business Approver", "Developer"],
                1,
                "terminal test note");

            Assert.True(setupResult.IsSuccess, setupResult.TopError.Description);
        }

        var result = request.TransitionTo(RequestStatus.UnderReview, [role], 1);

        Assert.Equal("Request.InvalidStatusTransition", result.TopError.Code);
    }

    private static Request CreateRequest() =>
        Request.Create(
            title: "Test request",
            description: "Test description",
            requestTypeId: 1,
            requestedById: 1,
            impactedModuleTypeId: 1,
            businessJustification: "Test justification").Value;
}
