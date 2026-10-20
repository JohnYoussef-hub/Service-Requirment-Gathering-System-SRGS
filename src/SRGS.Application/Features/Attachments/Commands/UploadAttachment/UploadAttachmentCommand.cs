using MediatR;
using SRGS.Application.Features.Attachments.Dtos;
using SRGS.Domain.Common.Results;

namespace SRGS.Application.Features.Attachments.Commands.UploadAttachment;

public sealed record UploadAttachmentCommand(
    int RequestId,
    string FileName,
    string? ClientContentType,
    byte[] FileData) : IRequest<Result<AttachmentDto>>;
