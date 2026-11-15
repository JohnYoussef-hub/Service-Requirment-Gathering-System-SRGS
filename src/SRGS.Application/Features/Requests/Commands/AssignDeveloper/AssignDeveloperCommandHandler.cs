using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using MechanicShop.Application.Common.Interfaces;
using SRGS.Application.Common.Errors;
using SRGS.Application.Common.Exceptions;
using SRGS.Application.Common.Interfaces;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Requests;

namespace SRGS.Application.Features.Requests.Commands.AssignDeveloper;

public sealed class AssignDeveloperCommandHandler(
    IAppDbContext context,
    ICurrentUser currentUser,
    HybridCache cache)
    : IRequestHandler<AssignDeveloperCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(AssignDeveloperCommand command, CancellationToken ct)
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

        if (!await context.Users.AnyAsync(user => user.Id == command.DeveloperId, ct))
        {
            return ApplicationErrors.UserNotFound;
        }

        var transitionResult = request.TransitionTo(
            SRGS.Domain.Requests.Enums.RequestStatus.Assigned,
            currentUser.Roles,
            actorId,
            $"Developer {command.DeveloperId} assigned.");

        if (transitionResult.IsError)
        {
            return transitionResult.Errors;
        }

        var assignmentResult = request.AssignDeveloper(command.DeveloperId);
        if (assignmentResult.IsError)
        {
            return assignmentResult.Errors;
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

        return assignmentResult.Value;
    }
}
