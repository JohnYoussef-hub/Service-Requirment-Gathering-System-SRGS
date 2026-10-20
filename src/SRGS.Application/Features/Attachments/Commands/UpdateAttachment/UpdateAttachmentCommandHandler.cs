using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using MechanicShop.Application.Common.Interfaces;
using SRGS.Application.Common.Errors;
using SRGS.Application.Common.Interfaces;
using SRGS.Application.Features.Attachments;
using SRGS.Application.Features.Attachments.Dtos;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Requests.Attachments;

namespace SRGS.Application.Features.Attachments.Commands.UpdateAttachment;

public sealed class UpdateAttachmentCommandHandler(
    IAppDbContext context,
    HybridCache cache,
    ICurrentUser currentUser)
    : IRequestHandler<UpdateAttachmentCommand, Result<AttachmentDto>>
{
    public async Task<Result<AttachmentDto>> Handle(UpdateAttachmentCommand command, CancellationToken ct)
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
            return AttachmentErrors.UpdateForbidden;
        }

        var file = AttachmentFile.Create(command.FileName, command.FileData);
        if (file.IsError)
        {
            return file.Errors;
        }

        var updateResult = attachment.UpdateFile(
            file.Value.FileName,
            file.Value.Extension,
            file.Value.ContentType,
            command.FileData);

        if (updateResult.IsError)
        {
            return updateResult.Errors;
        }

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync("request", ct);

        return await AttachmentFile.ToDtoAsync(context, attachment.Id, ct);
    }
}
