using SRGS.Domain.Common;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Requests.Notifications.Enums;

namespace SRGS.Domain.Requests.Notifications;

public sealed class Notification : Entity<int>
{
    public int RequestId { get; private set; }

    // EventType has no CHECK constraint in the DB, so it stays a free-form string
    // (e.g. "StatusChanged", "ApprovalDecided", "CommentAdded") rather than an enum —
    // enum it later if/when the set of event types is confirmed and locked down.
    public string EventType { get; private set; }
    public NotificationStatus Status { get; private set; }
    public int RecipientUserId { get; private set; }
    public DateTimeOffset? SentAtUtc { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Notification()
#pragma warning restore CS8618
    { }

    private Notification(int requestId, string eventType, int recipientUserId)
    {
        RequestId = requestId;
        EventType = eventType;
        RecipientUserId = recipientUserId;
        Status = NotificationStatus.Pending;
    }

    public static Result<Notification> Create(int requestId, string eventType, int recipientUserId)
    {
        if (requestId <= 0)
        {
            return NotificationErrors.RequestIdRequired;
        }

        if (string.IsNullOrWhiteSpace(eventType))
        {
            return NotificationErrors.EventTypeRequired;
        }

        if (recipientUserId <= 0)
        {
            return NotificationErrors.RecipientRequired;
        }

        return new Notification(requestId, eventType.Trim(), recipientUserId);
    }

    public Result<Updated> MarkSent()
    {
        if (Status != NotificationStatus.Pending)
        {
            return NotificationErrors.NotPending(Id);
        }

        Status = NotificationStatus.Sent;
        SentAtUtc = DateTimeOffset.UtcNow;

        return Result.Updated;
    }

    public Result<Updated> MarkFailed()
    {
        if (Status != NotificationStatus.Pending)
        {
            return NotificationErrors.NotPending(Id);
        }

        Status = NotificationStatus.Failed;

        return Result.Updated;
    }
}
