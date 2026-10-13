using MediatR;
using Microsoft.EntityFrameworkCore;
using SRGS.Application.Common.Interfaces;
using SRGS.Application.Features.Attachments.Dtos;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Requests.Attachments;

namespace SRGS.Application.Features.Attachments.Queries.DownloadAttachment;

public sealed class DownloadAttachmentQueryHandler(IAppDbContext context)
    : IRequestHandler<DownloadAttachmentQuery, Result<AttachmentDownloadDto>>
{
    public async Task<Result<AttachmentDownloadDto>> Handle(DownloadAttachmentQuery query, CancellationToken ct)
    {
        var attachment = await context.Attachments
            .AsNoTracking()
            .Where(a => a.Id == query.AttachmentId && a.RequestId == query.RequestId)
            .Select(a => new AttachmentDownloadDto
            {
                FileName = a.FileName,
                ContentType = a.MimeType,
                Data = a.FileData
            })
            .SingleOrDefaultAsync(ct);

        return attachment is null
            ? AttachmentErrors.NotFound(query.AttachmentId)
            : attachment;
    }
}
