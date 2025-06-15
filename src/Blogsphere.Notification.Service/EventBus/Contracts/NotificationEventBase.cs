using Blogsphere.Notification.Service.Models.Enums;

namespace Blogsphere.Notification.Service.EventBus.Contracts
{
    public abstract class NotificationEventBase
    {
        public DateTime EventDate { get; set; }
        public string CorrelationId { get; set; }
        public object AdditionalProperties { get; set; }
        public abstract NotificationTypes NotificationType { get; set; }
    }
}