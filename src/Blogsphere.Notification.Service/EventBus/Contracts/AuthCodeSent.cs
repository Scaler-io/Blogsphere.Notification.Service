using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blogsphere.Notification.Service.EventBus.Contracts;
using Blogsphere.Notification.Service.Models.Enums;

namespace Contracts.Events;
public class AuthCodeSent : NotificationEventBase
{
    public string Email { get; set; }
    public string Code { get; set; }
    public DateTime ExpirationTime { get; set; }
    public string CorrelationId { get; set; }
    public object AdditionalProperties { get; set; }
    public override NotificationTypes NotificationType { get; set; } = NotificationTypes.AuthCodeSent;
}
