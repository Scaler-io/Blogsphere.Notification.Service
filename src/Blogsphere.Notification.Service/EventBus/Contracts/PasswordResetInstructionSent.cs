using Blogsphere.Notification.Service.EventBus.Contracts;
using Blogsphere.Notification.Service.Models.Enums;

namespace Contracts.Events;
public class PasswordResetInstructionSent : NotificationEventBase
{
    public string Email { get; set; }
    public override NotificationTypes NotificationType { get; set; } = NotificationTypes.PasswordResentInstructionSent;
}
