using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using MechanicShop.Application.Common.Interfaces;
using SRGS.Application.Common.Errors;
using SRGS.Application.Common.Exceptions;
using SRGS.Application.Common.Interfaces;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Requests;
using SRGS.Domain.Requests.Approvals;
using SRGS.Domain.Requests.Approvals.Enums;
using SRGS.Domain.Requests.Enums;

namespace SRGS.Application.Features.Requests.Commands.DecideApproval;

public sealed class DecideApprovalCommandHandler(
    IAppDbContext context,
    ICurrentUser currentUser,
    HybridCache cache)
    : IRequestHandler<DecideApprovalCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(DecideApprovalCommand command, CancellationToken ct)
    {
        if (currentUser.UserId is not int approverId)
        {
            return ApplicationErrors.Unauthorized;
        }

        var request = await context.Requests.SingleOrDefaultAsync(r => r.Id == command.RequestId, ct);
        if (request is null)
        {
            return RequestErrors.NotFound(command.RequestId);
        }

        if (request.Status is not (RequestStatus.ManagementApproval or RequestStatus.StakeholdersApproval))
        {
            return RequestErrors.InvalidStatusTransition(request.Status, RequestStatus.StakeholdersApproval);
        }

        var approvalResult = Approval.Create(command.RequestId, approverId, command.ApprovalType);
        if (approvalResult.IsError)
        {
            return approvalResult.Errors;
        }

        var decisionResult = approvalResult.Value.Decide(command.Decision);
        if (decisionResult.IsError)
        {
            return decisionResult.Errors;
        }

        if (command.Decision == ApprovalDecisionStatus.Rejected)
        {
            var transitionResult = request.TransitionTo(
                RequestStatus.Rejected,
                currentUser.Roles,
                approverId,
                command.Note);

            if (transitionResult.IsError)
            {
                return transitionResult.Errors;
            }
        }
        else if (request.Status == RequestStatus.ManagementApproval)
        {
            var transitionResult = request.TransitionTo(
                RequestStatus.StakeholdersApproval,
                currentUser.Roles,
                approverId,
                command.Note);

            if (transitionResult.IsError)
            {
                return transitionResult.Errors;
            }
        }

        context.Approvals.Add(approvalResult.Value);
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return ApplicationErrors.ConcurrencyConflict;
        }
        await cache.RemoveByTagAsync("request", ct);

        return decisionResult.Value;
    }
}
