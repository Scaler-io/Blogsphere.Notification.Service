using Azure;
using Azure.Data.Tables;

namespace Blogsphere.Notification.Service.Entities;

public class EntityBase : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public string CorrelationId { get; set; }
    public ETag ETag { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}