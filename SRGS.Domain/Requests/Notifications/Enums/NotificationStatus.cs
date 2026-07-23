namespace SRGS.Domain.Requests.Notifications.Enums;

// Backed by CHECK CK_NOTIFICATION_status: ('Pending','Sent','Failed'). Default is Pending.
public enum NotificationStatus
{
    Pending,
    Sent,
    Failed
}
