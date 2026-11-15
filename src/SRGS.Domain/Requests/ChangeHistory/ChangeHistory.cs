using SRGS.Domain.Common;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Requests.Enums;
using SRGS.Domain.Requests.History.Enums;

namespace SRGS.Domain.Requests.History;

// Deliberately has no mutation methods: it's an append-only audit trail, so once created
// it should never be edited.
public sealed class ChangeHistory : Entity<int>
{
    public int RequestId { get; private set; }
    public ChangeOperation ChangeOperation { get; private set; }
    public ChangeType? ChangeType { get; private set; }
    public int ChangedById { get; private set; }
    public string? Comment { get; private set; }
    public DateTimeOffset ChangedAtUtc { get; private set; }
    public RequestStatus? OldStatus { get; private set; }
    public RequestStatus? NewStatus { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ChangeHistory()
#pragma warning restore CS8618
    { }

    private ChangeHistory(int requestId, ChangeOperation changeOperation, ChangeType? changeType, int changedById, string? comment)
    {
        RequestId = requestId;
        ChangeOperation = changeOperation;
        ChangeType = changeType;
        ChangedById = changedById;
        Comment = comment;
        ChangedAtUtc = DateTimeOffset.UtcNow;
    }

    private ChangeHistory(
        int requestId,
        int changedById,
        RequestStatus oldStatus,
        RequestStatus newStatus,
        string? comment)
        : this(requestId, ChangeOperation.Update,
            SRGS.Domain.Requests.History.Enums.ChangeType.Workflow, changedById, comment)
    {
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }

    public static Result<ChangeHistory> CreateStatusTransition(
        int requestId,
        RequestStatus oldStatus,
        RequestStatus newStatus,
        int changedById,
        string? comment)
    {
        if (requestId <= 0)
        {
            return ChangeHistoryErrors.RequestIdRequired;
        }

        if (changedById <= 0)
        {
            return ChangeHistoryErrors.ChangedByRequired;
        }

        if (!Enum.IsDefined(oldStatus) || !Enum.IsDefined(newStatus))
        {
            return ChangeHistoryErrors.StatusInvalid;
        }

        return new ChangeHistory(requestId, changedById, oldStatus, newStatus,
            string.IsNullOrWhiteSpace(comment) ? null : comment.Trim());
    }

    public static Result<ChangeHistory> Create(
        int requestId,
        ChangeOperation changeOperation,
        int changedById,
        ChangeType? changeType = null,
        string? comment = null)
    {
        if (requestId <= 0)
        {
            return ChangeHistoryErrors.RequestIdRequired;
        }

        if (!Enum.IsDefined(changeOperation))
        {
            return ChangeHistoryErrors.OperationInvalid;
        }

        if (changedById <= 0)
        {
            return ChangeHistoryErrors.ChangedByRequired;
        }

        return new ChangeHistory(requestId, changeOperation, changeType, changedById, string.IsNullOrWhiteSpace(comment) ? null : comment.Trim());
    }
}
