using SRGS.Domain.Common.Results;

namespace SRGS.Domain.Requests.Notifications;

public static class NotificationErrors
{
    public static Error RequestIdRequired =>
        Error.Validation("Notification.RequestId.Required", "Request id is required.");

    public static Error EventTypeRequired =>
        Error.Validation("Notification.EventType.Required", "Event type is required.");

    public static Error RecipientRequired =>
        Error.Validation("Notification.RecipientUserId.Required", "Recipient id is required.");

    public static Error NotPending(int id) => Error.Conflict(
        code: "Notification.NotPending",
        description: $"Notification '{id}' has already been sent or marked failed.");
}
