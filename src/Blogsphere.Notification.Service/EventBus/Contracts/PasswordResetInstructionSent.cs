using Blogsphere.Notification.Service.EventBus.Contracts;
using Blogsphere.Notification.Service.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Events;
public class PasswordResetInstructionSent : NotificationEventBase
{
    public string Email { get; set; }
    public override NotificationTypes NotificationType { get; set; } = NotificationTypes.PasswordResentInstructionSent;
}
