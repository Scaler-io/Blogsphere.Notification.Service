using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blogsphere.Notification.Service.EventBus.Contracts;
using Blogsphere.Notification.Service.Models.Enums;

namespace Contracts.Events;

public class PasswordResetOneTimeCodeSent : NotificationEventBase
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public override NotificationTypes NotificationType { get; set; } = NotificationTypes.PasswordResetOneTimeCodeSent;
}
