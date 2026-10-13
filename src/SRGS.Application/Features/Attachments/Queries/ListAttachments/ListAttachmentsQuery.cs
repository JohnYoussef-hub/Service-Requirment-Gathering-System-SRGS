using MediatR;
using SRGS.Application.Features.Attachments.Dtos;
using SRGS.Domain.Common.Results;

namespace SRGS.Application.Features.Attachments.Queries.ListAttachments;

public sealed record ListAttachmentsQuery(int RequestId) : IRequest<Result<List<AttachmentDto>>>;
