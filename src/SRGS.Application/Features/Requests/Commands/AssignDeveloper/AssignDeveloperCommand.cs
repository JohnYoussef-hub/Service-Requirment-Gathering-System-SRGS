using MediatR;
using SRGS.Domain.Common.Results;

namespace SRGS.Application.Features.Requests.Commands.AssignDeveloper;

public sealed record AssignDeveloperCommand(
    int RequestId,
    int DeveloperId) : IRequest<Result<Updated>>;
