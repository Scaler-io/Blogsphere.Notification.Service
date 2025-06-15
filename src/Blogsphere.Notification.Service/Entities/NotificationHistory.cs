namespace Blogsphere.Notification.Service.Entities;

public class NotificationHistory : EntityBase
{
    public string Subject { get; set; }
    public string RecipientEmail { get; set; }
    public string Data { get; set; }
    public string TemplateName { get; set; }
    public bool IsPublished { get; set; }
    public DateTime PublishTime { get; set; }
}