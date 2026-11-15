using MediatR;
using SRGS.Domain.Common.Results;

namespace SRGS.Application.Features.Requests.Commands.RespondToClarification;

public sealed record RespondToClarificationCommand(
    int RequestId,
    string Response) : IRequest<Result<Updated>>;
