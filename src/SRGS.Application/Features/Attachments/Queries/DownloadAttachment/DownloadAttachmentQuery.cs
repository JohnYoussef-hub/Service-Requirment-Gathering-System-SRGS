using MediatR;
using SRGS.Application.Features.Attachments.Dtos;
using SRGS.Domain.Common.Results;

namespace SRGS.Application.Features.Attachments.Queries.DownloadAttachment;

public sealed record DownloadAttachmentQuery(int RequestId, int AttachmentId)
    : IRequest<Result<AttachmentDownloadDto>>;
