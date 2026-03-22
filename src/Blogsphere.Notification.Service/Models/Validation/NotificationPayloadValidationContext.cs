using Microsoft.Extensions.Logging;

namespace Blogsphere.Notification.Service.Models.Validation;

public sealed record NotificationPayloadValidationContext(
    string PartitionKey,
    string MessageId,
    int PayloadByteLength,
    string EventName,
    ILogger Logger,
    string CorrelationId);
