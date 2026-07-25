using SRGS.Application.Features.Requests.Dtos;
using SRGS.Domain.Requests;

namespace SRGS.Application.Features.Requests.Mappers;

public static class RequestMapper
{
    /// <summary>
    /// Maps the fields Request actually knows about. Request holds ids only (no
    /// navigation to User/RequestType/ModuleType — that's a deliberate domain decision,
    /// not an oversight), so the *Name fields below always come back null here.
    /// Use this when you already have a tracked/loaded Request in hand (e.g. right after
    /// Create() inside a command handler) and don't need the names for that response.
    /// For a read endpoint that needs names, don't route through this mapper at all —
    /// see ToRequestDto() in the query handler note below.
    /// </summary>
    public static RequestDto ToDto(this Request entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new RequestDto
        {
            RequestId = entity.Id,
            RequestCode = entity.RequestCode,
            CreatedDate = entity.CreatedDate,
            Title = entity.Title,
            Description = entity.Description,
            RequestTypeId = entity.RequestTypeId,
            RequestedById = entity.RequestedById,
            ImpactedModuleTypeId = entity.ImpactedModuleTypeId,
            CurrentBehavior = entity.CurrentBehavior,
            ExpectedBehavior = entity.ExpectedBehavior,
            BusinessJustification = entity.BusinessJustification,
            Priority = entity.Priority,
            Status = entity.Status,
            Phase = entity.Phase,
            AssignedDeveloperId = entity.AssignedDeveloperId,
            AssignedBusinessAnalystId = entity.AssignedBusinessAnalystId,
            EstimatedEffort = entity.EstimatedEffort,
            ActualStart = entity.ActualStart,
            ActualEnd = entity.ActualEnd,
            OpenTimeUtc = entity.OpenTimeUtc,
            CloseTimeUtc = entity.CloseTimeUtc,
            DevelopmentProgress = entity.DevelopmentProgress,
            UatResult = entity.UatResult
        };
    }

    public static List<RequestDto> ToDtos(this IEnumerable<Request> entities)
    {
        return [.. entities.Select(e => e.ToDto())];
    }
}
