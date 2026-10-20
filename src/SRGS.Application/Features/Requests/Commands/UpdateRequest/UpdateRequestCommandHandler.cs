using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using MechanicShop.Application.Common.Interfaces;
using SRGS.Application.Common.Errors;
using SRGS.Application.Common.Interfaces;
using SRGS.Application.Features.Requests.Dtos;
using SRGS.Application.Features.Requests.Mappers;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Requests;

namespace SRGS.Application.Features.Requests.Commands.UpdateRequest;

public sealed class UpdateRequestCommandHandler(
    IAppDbContext context,
    HybridCache cache,
    ICurrentUser currentUser)
    : IRequestHandler<UpdateRequestCommand, Result<RequestDto>>
{
    public async Task<Result<RequestDto>> Handle(UpdateRequestCommand command, CancellationToken ct)
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
            return RequestErrors.UpdateForbidden;
        }

        var detailsResult = request.UpdateDetails(
            command.Title,
            command.Description,
            command.BusinessJustification,
            command.CurrentBehavior,
            command.ExpectedBehavior);

        if (detailsResult.IsError)
        {
            return detailsResult.Errors;
        }

        var priorityResult = request.UpdatePriority(command.Priority);
        if (priorityResult.IsError)
        {
            return priorityResult.Errors;
        }

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync("request", ct);

        return request.ToDto();
    }
}
