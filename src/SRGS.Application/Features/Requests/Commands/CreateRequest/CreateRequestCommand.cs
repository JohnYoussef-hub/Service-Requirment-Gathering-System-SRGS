using MediatR;
using SRGS.Application.Features.Requests.Dtos;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Requests.Enums;

namespace SRGS.Application.Features.Requests.Commands.CreateRequest;


public sealed record CreateRequestCommand(
    string Title,
    string Description,
    int RequestTypeId,
    int RequestedById,
    int ModuleTypeId,
    string? CurrentBehavior,
    string? ExpectedBehavior,
    string BusinessJustification,
    RequestPriority Priority) : IRequest<Result<RequestDto>>;