using Blogsphere.Notification.Service.EventBus.Contracts;
using Blogsphere.Notification.Service.Models.Enums;

namespace Contracts.Events;

public class PhoneVerificationCodeSent : NotificationEventBase
{
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Code { get; set; }
    public override NotificationTypes NotificationType { get; set; } = NotificationTypes.PhoneVerificationCodeSent;
}
