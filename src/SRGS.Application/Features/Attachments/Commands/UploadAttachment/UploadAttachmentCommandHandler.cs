using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using MechanicShop.Application.Common.Interfaces;
using SRGS.Application.Common.Errors;
using SRGS.Application.Common.Interfaces;
using SRGS.Application.Features.Attachments;
using SRGS.Application.Features.Attachments.Dtos;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Requests;
using SRGS.Domain.Requests.Attachments;

namespace SRGS.Application.Features.Attachments.Commands.UploadAttachment;

public sealed class UploadAttachmentCommandHandler(
    ILogger<UploadAttachmentCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache,
    ICurrentUser currentUser)
    : IRequestHandler<UploadAttachmentCommand, Result<AttachmentDto>>
{
    public async Task<Result<AttachmentDto>> Handle(UploadAttachmentCommand command, CancellationToken ct)
    {
        if (currentUser.UserId is not int uploaderId)
        {
            logger.LogError("The current user does not have a valid user id claim.");
            return ApplicationErrors.Unauthorized;
        }

        if (!await context.Requests.AnyAsync(r => r.Id == command.RequestId, ct))
        {
            return RequestErrors.NotFound(command.RequestId);
        }

        var file = AttachmentFile.Create(command.FileName, command.FileData);
        if (file.IsError)
        {
            return file.Errors;
        }

        var createResult = Attachment.Create(
            command.RequestId,
            uploaderId,
            file.Value.FileName,
            file.Value.ContentType,
            command.FileData,
            file.Value.Extension);

        if (createResult.IsError)
        {
            logger.LogWarning("Failed to create attachment: {Error}", createResult.TopError.Description);
            return createResult.Errors;
        }

        var attachment = createResult.Value;
        context.Attachments.Add(attachment);
        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync("request", ct);

        return await AttachmentFile.ToDtoAsync(context, attachment.Id, ct);
    }
}
