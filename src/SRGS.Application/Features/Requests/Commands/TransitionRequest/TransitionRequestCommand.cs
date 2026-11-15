using MediatR;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Requests.Enums;

namespace SRGS.Application.Features.Requests.Commands.TransitionRequest;

public sealed record TransitionRequestCommand(
    int RequestId,
    RequestStatus TargetStatus,
    string? Note) : IRequest<Result<Updated>>;
