using Blogsphere.Notification.Service.EventBus.Contracts;
using Blogsphere.Notification.Service.Models.Enums;

namespace Contracts.Events;

public sealed class UserInvitationSent : NotificationEventBase
{
    public string UserId { get; private set; }
    public string UserName { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; set; }
    public string Email { get; private set; }
    public override NotificationTypes NotificationType { get; set;} = NotificationTypes.UserInvitation;
}