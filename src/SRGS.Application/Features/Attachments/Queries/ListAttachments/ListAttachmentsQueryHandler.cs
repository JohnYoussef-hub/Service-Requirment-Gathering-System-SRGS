using MediatR;
using Microsoft.EntityFrameworkCore;
using SRGS.Application.Common.Interfaces;
using SRGS.Application.Features.Attachments.Dtos;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Requests;

namespace SRGS.Application.Features.Attachments.Queries.ListAttachments;

public sealed class ListAttachmentsQueryHandler(IAppDbContext context)
    : IRequestHandler<ListAttachmentsQuery, Result<List<AttachmentDto>>>
{
    public async Task<Result<List<AttachmentDto>>> Handle(ListAttachmentsQuery query, CancellationToken ct)
    {
        if (!await context.Requests.AnyAsync(r => r.Id == query.RequestId, ct))
        {
            return RequestErrors.NotFound(query.RequestId);
        }

        var attachments = await context.Attachments
            .AsNoTracking()
            .Where(a => a.RequestId == query.RequestId)
            .Select(a => new AttachmentDto
            {
                AttachmentId = a.Id,
                FileName = a.FileName,
                ContentType = a.MimeType,
                Size = a.FileSize,
                UploadedById = a.UploadedByUserId,
                UploadedByName = context.Users
                    .Where(u => u.Id == a.UploadedByUserId)
                    .Select(u => u.FirstName + " " + u.FamilyName)
                    .First(),
                UploadedAt = a.UploadedAtUtc
            })
            .ToListAsync(ct);

        return attachments;
    }
}
