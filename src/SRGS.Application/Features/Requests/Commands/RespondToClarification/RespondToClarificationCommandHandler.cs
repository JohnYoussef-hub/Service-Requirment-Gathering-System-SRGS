using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using MechanicShop.Application.Common.Interfaces;
using SRGS.Application.Common.Errors;
using SRGS.Application.Common.Exceptions;
using SRGS.Application.Common.Interfaces;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Requests;
using SRGS.Domain.Requests.Enums;

namespace SRGS.Application.Features.Requests.Commands.RespondToClarification;

public sealed class RespondToClarificationCommandHandler(
    IAppDbContext context,
    ICurrentUser currentUser,
    HybridCache cache)
    : IRequestHandler<RespondToClarificationCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(RespondToClarificationCommand command, CancellationToken ct)
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

        var result = request.TransitionTo(
            RequestStatus.UnderReview,
            currentUser.Roles,
            actorId,
            command.Response);

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
