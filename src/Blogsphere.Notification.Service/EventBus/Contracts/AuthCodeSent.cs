using Blogsphere.Notification.Service.EventBus.Contracts;
using Blogsphere.Notification.Service.Models.Enums;

namespace Contracts.Events;
public class AuthCodeSent : NotificationEventBase
{
    public string Email { get; set; }
    public string Code { get; set; }
    public DateTime ExpirationTime { get; set; }
    public override NotificationTypes NotificationType { get; set; } = NotificationTypes.AuthCodeSent;
}
