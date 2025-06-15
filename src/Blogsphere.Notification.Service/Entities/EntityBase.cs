namespace Blogsphere.Notification.Service.Entities;

public class EntityBase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CorrelationId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}