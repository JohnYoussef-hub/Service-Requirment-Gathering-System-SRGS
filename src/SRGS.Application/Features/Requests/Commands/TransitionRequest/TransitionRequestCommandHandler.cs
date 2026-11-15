using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using MechanicShop.Application.Common.Interfaces;
using SRGS.Application.Common.Errors;
using SRGS.Application.Common.Exceptions;
using SRGS.Application.Common.Interfaces;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Requests;

namespace SRGS.Application.Features.Requests.Commands.TransitionRequest;

public sealed class TransitionRequestCommandHandler(
    IAppDbContext context,
    ICurrentUser currentUser,
    HybridCache cache)
    : IRequestHandler<TransitionRequestCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(TransitionRequestCommand command, CancellationToken ct)
    {
        if (currentUser.UserId is not int actorId)
        {
            return ApplicationErrors.Unauthorized;
        }

        var request = await context.Requests.SingleOrDefaultAsync(r => r.Id == command.RequestId, ct);
        if (request is null)
        {
            return RequestErrors.NotFound(command.RequestId);
        }

        if (command.TargetStatus is SRGS.Domain.Requests.Enums.RequestStatus.Assigned
            or SRGS.Domain.Requests.Enums.RequestStatus.StakeholdersApproval
            || command.TargetStatus == SRGS.Domain.Requests.Enums.RequestStatus.Rejected
                && (request.Status is SRGS.Domain.Requests.Enums.RequestStatus.ManagementApproval
                    or SRGS.Domain.Requests.Enums.RequestStatus.StakeholdersApproval))
        {
            return RequestErrors.TransitionRequiresData(command.TargetStatus);
        }

        var result = request.TransitionTo(command.TargetStatus, currentUser.Roles, actorId, command.Note);
        if (result.IsError)
        {
            return result.Errors;
        }

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return ApplicationErrors.ConcurrencyConflict;
        }
        await cache.RemoveByTagAsync("request", ct);

        return result.Value;
    }
}
