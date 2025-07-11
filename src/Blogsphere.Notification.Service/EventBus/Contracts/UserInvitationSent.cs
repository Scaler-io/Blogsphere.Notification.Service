using Blogsphere.Notification.Service.EventBus.Contracts;
using Blogsphere.Notification.Service.Models.Enums;

namespace Contracts.Events;

public sealed class UserInvitationSent : NotificationEventBase
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public override NotificationTypes NotificationType { get; set;} = NotificationTypes.UserInvitation;
}