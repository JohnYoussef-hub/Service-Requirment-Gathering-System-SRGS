using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using MechanicShop.Application.Common.Interfaces;
using SRGS.Application.Common.Errors;
using SRGS.Application.Common.Interfaces;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Requests.Attachments;

namespace SRGS.Application.Features.Attachments.Commands.DeleteAttachment;

public sealed class DeleteAttachmentCommandHandler(
    IAppDbContext context,
    HybridCache cache,
    ICurrentUser currentUser)
    : IRequestHandler<DeleteAttachmentCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(DeleteAttachmentCommand command, CancellationToken ct)
    {
        if (currentUser.UserId is not int userId)
        {
            return ApplicationErrors.Unauthorized;
        }

        var attachment = await context.Attachments
            .SingleOrDefaultAsync(a => a.Id == command.AttachmentId && a.RequestId == command.RequestId, ct);

        if (attachment is null)
        {
            return AttachmentErrors.NotFound(command.AttachmentId);
        }

        if (attachment.UploadedByUserId != userId)
        {
            return AttachmentErrors.DeleteForbidden;
        }

        context.Attachments.Remove(attachment);
        await context.SaveChangesAsync(ct);
        await cache.RemoveAsync("requests", ct);

        return Result.Deleted;
    }
}
