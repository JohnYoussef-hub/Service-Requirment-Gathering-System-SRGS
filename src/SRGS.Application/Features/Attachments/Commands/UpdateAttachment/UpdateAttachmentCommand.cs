using MediatR;
using SRGS.Application.Features.Attachments.Dtos;
using SRGS.Domain.Common.Results;

namespace SRGS.Application.Features.Attachments.Commands.UpdateAttachment;

public sealed record UpdateAttachmentCommand(
    int RequestId,
    int AttachmentId,
    string FileName,
    string? ClientContentType,
    byte[] FileData) : IRequest<Result<AttachmentDto>>;
