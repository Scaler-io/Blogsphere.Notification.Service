using Blogsphere.Notification.Service.EventBus.Contracts;
using Blogsphere.Notification.Service.Models.Enums;

namespace Contracts.Events;

public class ManagementUserWelcomeEmailSent : NotificationEventBase
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public override NotificationTypes NotificationType { get; set; } = NotificationTypes.ManagementUserWelcomeEmailSent;
}
