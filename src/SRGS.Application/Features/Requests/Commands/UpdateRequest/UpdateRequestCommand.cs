using MediatR;
using SRGS.Application.Features.Requests.Dtos;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Requests.Enums;

namespace SRGS.Application.Features.Requests.Commands.UpdateRequest;

public sealed record UpdateRequestCommand(
    int RequestId,
    string Title,
    string Description,
    string? CurrentBehavior,
    string? ExpectedBehavior,
    string BusinessJustification,
    RequestPriority Priority) : IRequest<Result<RequestDto>>;
