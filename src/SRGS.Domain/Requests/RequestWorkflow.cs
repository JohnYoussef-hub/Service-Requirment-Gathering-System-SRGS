using SRGS.Domain.Requests.Enums;

namespace SRGS.Domain.Requests;

public static class RequestWorkflow
{
    private static readonly IReadOnlyDictionary<(RequestStatus From, RequestStatus To), IReadOnlySet<string>> AllowedTransitions =
        new Dictionary<(RequestStatus From, RequestStatus To), IReadOnlySet<string>>
        {
            [(RequestStatus.Submitted, RequestStatus.UnderReview)] = Roles("BA"),
            [(RequestStatus.Submitted, RequestStatus.Cancelled)] = Roles("Requester"),
            [(RequestStatus.UnderReview, RequestStatus.ClarificationRequired)] = Roles("BA"),
            [(RequestStatus.UnderReview, RequestStatus.Analysis)] = Roles("BA"),
            [(RequestStatus.UnderReview, RequestStatus.Rejected)] = Roles("BA"),
            [(RequestStatus.ClarificationRequired, RequestStatus.UnderReview)] = Roles("Requester"),
            [(RequestStatus.ClarificationRequired, RequestStatus.Cancelled)] = Roles("Requester"),
            [(RequestStatus.Analysis, RequestStatus.ManagementApproval)] = Roles("BA"),
            [(RequestStatus.Analysis, RequestStatus.ClarificationRequired)] = Roles("BA"),
            [(RequestStatus.Analysis, RequestStatus.Rejected)] = Roles("BA"),
            [(RequestStatus.ManagementApproval, RequestStatus.StakeholdersApproval)] = Roles("Business Approver"),
            [(RequestStatus.ManagementApproval, RequestStatus.ClarificationRequired)] = Roles("Business Approver"),
            [(RequestStatus.ManagementApproval, RequestStatus.Rejected)] = Roles("Business Approver"),
            [(RequestStatus.StakeholdersApproval, RequestStatus.Assigned)] = Roles("Business Approver"),
            [(RequestStatus.StakeholdersApproval, RequestStatus.ClarificationRequired)] = Roles("Business Approver"),
            [(RequestStatus.StakeholdersApproval, RequestStatus.Rejected)] = Roles("Business Approver"),
            [(RequestStatus.Assigned, RequestStatus.InDevelopment)] = Roles("Developer"),
            [(RequestStatus.InDevelopment, RequestStatus.InternalTesting)] = Roles("Developer"),
            [(RequestStatus.InternalTesting, RequestStatus.UAT)] = Roles("Developer"),
            [(RequestStatus.InternalTesting, RequestStatus.InDevelopment)] = Roles("Developer"),
            [(RequestStatus.UAT, RequestStatus.Deployed)] = Roles("Developer"),
            [(RequestStatus.UAT, RequestStatus.InDevelopment)] = Roles("Requester"),
            [(RequestStatus.Deployed, RequestStatus.Closed)] = Roles("BA", "Requester")
        };

    public static bool IsAllowed(RequestStatus from, RequestStatus to, IEnumerable<string> actorRoles)
    {
        return AllowedTransitions.TryGetValue((from, to), out var allowedRoles)
            && actorRoles.Any(actorRole => allowedRoles.Contains(actorRole));
    }

    public static bool IsDefined(RequestStatus from, RequestStatus to)
    {
        return AllowedTransitions.ContainsKey((from, to));
    }

    private static IReadOnlySet<string> Roles(params string[] roles) =>
        new HashSet<string>(roles, StringComparer.OrdinalIgnoreCase);
}
