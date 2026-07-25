using SRGS.Domain.Requests.Enums;

namespace SRGS.Application.Features.Requests.Dtos;

public sealed class RequestDto
{
    public int RequestId { get; init; }
    public string? RequestCode { get; init; }
    public DateOnly CreatedDate { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    public int RequestTypeId { get; init; }
    public string RequestTypeName { get; init; } = string.Empty;

    public int RequestedById { get; init; }
    public string RequestedByName { get; init; } = string.Empty;

    public int ImpactedModuleTypeId { get; init; }
    public string ImpactedModuleTypeName { get; init; } = string.Empty;

    public string? CurrentBehavior { get; init; }
    public string? ExpectedBehavior { get; init; }

    public string BusinessJustification { get; init; } = string.Empty;

    public RequestPriority Priority { get; init; }
    public RequestStatus Status { get; init; }
    public RequestPhase Phase { get; init; }

    public int? AssignedDeveloperId { get; init; }
    public string? AssignedDeveloperName { get; init; }

    public int? AssignedBusinessAnalystId { get; init; }
    public string? AssignedBusinessAnalystName { get; init; }

    public decimal? EstimatedEffort { get; init; }
    public DateTimeOffset? ActualStart { get; init; }
    public DateTimeOffset? ActualEnd { get; init; }
    public DateTimeOffset OpenTimeUtc { get; init; }
    public DateTimeOffset? CloseTimeUtc { get; init; }
    public byte? DevelopmentProgress { get; init; }
    public UatResult? UatResult { get; init; }
}