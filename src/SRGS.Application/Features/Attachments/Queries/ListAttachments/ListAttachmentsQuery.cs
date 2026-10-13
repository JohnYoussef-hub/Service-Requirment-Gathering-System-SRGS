using MediatR;
using SRGS.Application.Common.Interfaces;
using SRGS.Application.Features.Attachments.Dtos;
using SRGS.Domain.Common.Results;

namespace SRGS.Application.Features.Attachments.Queries.ListAttachments;

public sealed record ListAttachmentsQuery(int RequestId) : ICachedQuery<Result<List<AttachmentDto>>>
{
    public string CacheKey => $"Request-{RequestId}-Attachments";

    public string[] Tags => ["request"];

    public TimeSpan Expiration => TimeSpan.FromMinutes(15);
}
