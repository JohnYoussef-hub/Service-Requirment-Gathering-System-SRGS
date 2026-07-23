using SRGS.Application.Features.Requests.Dtos;
using SRGS.Domain.Requests;

namespace SRGS.Application.Features.Requests.Mappers;


public static class RequestMapper
{
    public static RequestDto ToDto(this Request request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        return new RequestDto
        {
            Id = request.Id,
            Title = request.Title,
            Description = request.Description,
            BusinessJustification = request.BusinessJustification,
            Priority = request.Priority,
            RequestTypeId = request.RequestTypeId,
            RequestedById = request.RequestedById,
            ImpactedModuleTypeId = request.ImpactedModuleTypeId,
            CurrentBehavior = request.CurrentBehavior,
            ExpectedBehavior = request.ExpectedBehavior
        };
    }
}