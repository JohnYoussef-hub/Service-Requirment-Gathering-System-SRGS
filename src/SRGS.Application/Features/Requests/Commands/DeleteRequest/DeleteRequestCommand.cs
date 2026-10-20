using MediatR;
using SRGS.Domain.Common.Results;

namespace SRGS.Application.Features.Requests.Commands.DeleteRequest;

public sealed record DeleteRequestCommand(int RequestId) : IRequest<Result<Deleted>>;
