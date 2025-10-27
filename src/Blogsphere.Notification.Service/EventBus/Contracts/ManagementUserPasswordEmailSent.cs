using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blogsphere.Notification.Service.EventBus.Contracts;
using Blogsphere.Notification.Service.Models.Enums;

namespace Contracts.Events;

public class ManagementUserPasswordEmailSent : NotificationEventBase
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public override NotificationTypes NotificationType { get; set; } = NotificationTypes.ManagementUserPasswordEmailSent;
}
