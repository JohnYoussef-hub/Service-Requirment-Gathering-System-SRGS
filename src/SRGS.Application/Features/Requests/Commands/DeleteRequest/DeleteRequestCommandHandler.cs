using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using MechanicShop.Application.Common.Interfaces;
using SRGS.Application.Common.Errors;
using SRGS.Application.Common.Exceptions;
using SRGS.Application.Common.Interfaces;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Requests;

namespace SRGS.Application.Features.Requests.Commands.DeleteRequest;

public sealed class DeleteRequestCommandHandler(
    IAppDbContext context,
    HybridCache cache,
    ICurrentUser currentUser)
    : IRequestHandler<DeleteRequestCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(DeleteRequestCommand command, CancellationToken ct)
    {
        if (currentUser.UserId is not int userId)
        {
            return ApplicationErrors.Unauthorized;
        }

        var request = await context.Requests
            .SingleOrDefaultAsync(r => r.Id == command.RequestId, ct);

        if (request is null)
        {
            return RequestErrors.NotFound(command.RequestId);
        }

        if (request.RequestedById != userId)
        {
            return RequestErrors.DeleteForbidden;
        }

        context.Requests.Remove(request);
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return ApplicationErrors.ConcurrencyConflict;
        }
        await cache.RemoveByTagAsync("request", ct);

        return Result.Deleted;
    }
}
