using MediatR;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Requests.Approvals.Enums;

namespace SRGS.Application.Features.Requests.Commands.DecideApproval;

public sealed record DecideApprovalCommand(
    int RequestId,
    ApprovalType ApprovalType,
    ApprovalDecisionStatus Decision,
    string? Note) : IRequest<Result<Updated>>;
