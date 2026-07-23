using SRGS.Domain.Common;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Requests.Enums;
using SRGS.Domain.Requests.Events;

namespace SRGS.Domain.Requests;

/// <summary>
/// Aggregate root for the REQUEST table. RequestCode is deliberately not settable from
/// here — it's a DB-computed PERSISTED column (RF-YYYYMMDD-NNNN), so the domain treats it
/// as read-only, populated by EF Core on read/insert, never assigned in Create().
/// </summary>
public sealed class Request : Entity<int>
{
    public string? RequestCode { get; private set; }
    public DateOnly CreatedDate { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public int RequestTypeId { get; private set; }
    public int RequestedById { get; private set; }
    public int ImpactedModuleTypeId { get; private set; }
    public string? CurrentBehavior { get; private set; }
    public string? ExpectedBehavior { get; private set; }
    public string BusinessJustification { get; private set; }
    public RequestPriority Priority { get; private set; }
    public RequestStatus Status { get; private set; }
    public RequestPhase Phase { get; private set; }
    public int? AssignedDeveloperId { get; private set; }
    public int? AssignedBusinessAnalystId { get; private set; }
    public decimal? EstimatedEffort { get; private set; }
    public DateTimeOffset? ActualStart { get; private set; }
    public DateTimeOffset? ActualEnd { get; private set; }
    public DateTimeOffset OpenTimeUtc { get; private set; }
    public DateTimeOffset? CloseTimeUtc { get; private set; }
    public byte? DevelopmentProgress { get; private set; }
    public UatResult? UatResult { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Request()
#pragma warning restore CS8618
    { }

    private Request(
        string title,
        string description,
        int requestTypeId,
        int requestedById,
        int impactedModuleTypeId,
        string businessJustification,
        RequestPriority priority,
        string? currentBehavior,
        string? expectedBehavior)
    {
        CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow);
        Title = title;
        Description = description;
        RequestTypeId = requestTypeId;
        RequestedById = requestedById;
        ImpactedModuleTypeId = impactedModuleTypeId;
        BusinessJustification = businessJustification;
        Priority = priority;
        CurrentBehavior = currentBehavior;
        ExpectedBehavior = expectedBehavior;
        Status = RequestStatus.UnderAnalysis;
        Phase = RequestPhase.Logging;
        OpenTimeUtc = DateTimeOffset.UtcNow;
    }

    public static Result<Request> Create(
        string title,
        string description,
        int requestTypeId,
        int requestedById,
        int impactedModuleTypeId,
        string businessJustification,
        RequestPriority priority = RequestPriority.Low,
        string? currentBehavior = null,
        string? expectedBehavior = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return RequestErrors.TitleRequired;
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return RequestErrors.DescriptionRequired;
        }

        if (requestTypeId <= 0)
        {
            return RequestErrors.RequestTypeRequired;
        }

        if (requestedById <= 0)
        {
            return RequestErrors.RequesterRequired;
        }

        if (impactedModuleTypeId <= 0)
        {
            return RequestErrors.ModuleTypeRequired;
        }

        if (string.IsNullOrWhiteSpace(businessJustification))
        {
            return RequestErrors.BusinessJustificationRequired;
        }

        if (!Enum.IsDefined(priority))
        {
            return RequestErrors.PriorityInvalid;
        }

        var request = new Request(
            title.Trim(),
            description.Trim(),
            requestTypeId,
            requestedById,
            impactedModuleTypeId,
            businessJustification.Trim(),
            priority,
            string.IsNullOrWhiteSpace(currentBehavior) ? null : currentBehavior.Trim(),
            string.IsNullOrWhiteSpace(expectedBehavior) ? null : expectedBehavior.Trim());

        request.AddDomainEvent(new RequestCreated { RequestId = request.Id });
        request.AddDomainEvent(new RequestCollectionModified());

        return request;
    }

    public bool IsEditable => Status is not (RequestStatus.Completed or RequestStatus.Cancelled or RequestStatus.Rejected);

    public Result<Updated> UpdateDetails(string title, string description, string businessJustification, string? currentBehavior, string? expectedBehavior)
    {
        if (!IsEditable)
        {
            return RequestErrors.ReadOnly(Id);
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            return RequestErrors.TitleRequired;
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return RequestErrors.DescriptionRequired;
        }

        if (string.IsNullOrWhiteSpace(businessJustification))
        {
            return RequestErrors.BusinessJustificationRequired;
        }

        Title = title.Trim();
        Description = description.Trim();
        BusinessJustification = businessJustification.Trim();
        CurrentBehavior = string.IsNullOrWhiteSpace(currentBehavior) ? null : currentBehavior.Trim();
        ExpectedBehavior = string.IsNullOrWhiteSpace(expectedBehavior) ? null : expectedBehavior.Trim();

        return Result.Updated;
    }

    public Result<Updated> UpdatePriority(RequestPriority priority)
    {
        if (!IsEditable)
        {
            return RequestErrors.ReadOnly(Id);
        }

        if (!Enum.IsDefined(priority))
        {
            return RequestErrors.PriorityInvalid;
        }

        Priority = priority;

        return Result.Updated;
    }

    public Result<Updated> AssignDeveloper(int developerId)
    {
        if (!IsEditable)
        {
            return RequestErrors.ReadOnly(Id);
        }

        if (developerId <= 0)
        {
            return RequestErrors.DeveloperRequired;
        }

        AssignedDeveloperId = developerId;

        AddDomainEvent(new RequestAssignedToDeveloper { RequestId = Id, DeveloperId = developerId });

        return Result.Updated;
    }

    public Result<Updated> AssignBusinessAnalyst(int businessAnalystId)
    {
        if (!IsEditable)
        {
            return RequestErrors.ReadOnly(Id);
        }

        if (businessAnalystId <= 0)
        {
            return RequestErrors.BusinessAnalystRequired;
        }

        AssignedBusinessAnalystId = businessAnalystId;

        AddDomainEvent(new RequestAssignedToBusinessAnalyst { RequestId = Id, BusinessAnalystId = businessAnalystId });

        return Result.Updated;
    }

    public Result<Updated> SetEstimatedEffort(decimal estimatedEffort)
    {
        if (!IsEditable)
        {
            return RequestErrors.ReadOnly(Id);
        }

        if (estimatedEffort < 0)
        {
            return RequestErrors.EstimatedEffortInvalid;
        }

        EstimatedEffort = estimatedEffort;

        return Result.Updated;
    }

    public Result<Updated> UpdateDevelopmentProgress(byte percent)
    {
        if (percent > 100)
        {
            return RequestErrors.DevelopmentProgressInvalid;
        }

        DevelopmentProgress = percent;

        return Result.Updated;
    }

    public Result<Updated> RecordActualStart(DateTimeOffset startedAtUtc)
    {
        ActualStart = startedAtUtc;

        return Result.Updated;
    }

    public Result<Updated> RecordActualEnd(DateTimeOffset endedAtUtc)
    {
        if (ActualStart is not null && endedAtUtc < ActualStart)
        {
            return RequestErrors.InvalidTiming;
        }

        ActualEnd = endedAtUtc;

        return Result.Updated;
    }

    public Result<Updated> SetUatResult(UatResult result)
    {
        if (!Enum.IsDefined(result))
        {
            return RequestErrors.UatResultInvalid;
        }

        UatResult = result;

        return Result.Updated;
    }

    public bool CanTransitionTo(RequestStatus newStatus)
    {
        return (Status, newStatus) switch
        {
            (RequestStatus.UnderAnalysis, RequestStatus.PendingApproval) => true,
            (RequestStatus.PendingApproval, RequestStatus.Approved) => true,
            (RequestStatus.PendingApproval, RequestStatus.Rejected) => true,
            (RequestStatus.PendingApproval, RequestStatus.UnderAnalysis) => true,
            (RequestStatus.Approved, RequestStatus.InDevelopment) => true,
            (RequestStatus.InDevelopment, RequestStatus.InUat) => true,
            (RequestStatus.InDevelopment, RequestStatus.OnHold) => true,
            (RequestStatus.InUat, RequestStatus.InDevelopment) => true,
            (RequestStatus.InUat, RequestStatus.Completed) => true,
            (RequestStatus.OnHold, RequestStatus.InDevelopment) => true,
            (_, RequestStatus.Cancelled) when Status is not (RequestStatus.Completed or RequestStatus.Cancelled or RequestStatus.Rejected) => true,
            _ => false
        };
    }

    public Result<Updated> UpdateStatus(RequestStatus newStatus)
    {
        if (!CanTransitionTo(newStatus))
        {
            return RequestErrors.InvalidStatusTransition(Status, newStatus);
        }

        var oldStatus = Status;
        Status = newStatus;

        AddDomainEvent(new RequestStatusChanged { RequestId = Id, OldStatus = oldStatus, NewStatus = newStatus });
        AddDomainEvent(new RequestCollectionModified());

        if (newStatus == RequestStatus.Completed)
        {
            CloseTimeUtc = DateTimeOffset.UtcNow;
            AddDomainEvent(new RequestCompleted { RequestId = Id });
        }

        return Result.Updated;
    }

    public Result<Updated> UpdatePhase(RequestPhase newPhase)
    {
        if (Phase == RequestPhase.Closed)
        {
            return RequestErrors.ReadOnly(Id);
        }

        if (!Enum.IsDefined(newPhase))
        {
            return RequestErrors.PhaseInvalid;
        }

        var oldPhase = Phase;
        Phase = newPhase;

        AddDomainEvent(new RequestPhaseChanged { RequestId = Id, OldPhase = oldPhase, NewPhase = newPhase });

        return Result.Updated;
    }
}
