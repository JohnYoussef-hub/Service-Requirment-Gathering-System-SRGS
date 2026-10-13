using MediatR;
using SRGS.Domain.Common.Results;

namespace SRGS.Application.Features.Attachments.Commands.DeleteAttachment;

public sealed record DeleteAttachmentCommand(int RequestId, int AttachmentId) : IRequest<Result<Deleted>>;
