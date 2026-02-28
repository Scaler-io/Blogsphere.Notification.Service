using Blogsphere.Notification.Service.Configurations;
using Blogsphere.Notification.Service.Data.Storage;
using Blogsphere.Notification.Service.Entities;
using Blogsphere.Notification.Service.Extensions;
using Blogsphere.Notification.Service.Models.Constants;
using Blogsphere.Notification.Service.Models.Notification;
using Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Blogsphere.Notification.Service.EventBus.Consumers;

public class ManagementUserWelcomeEmailSentConsumer(
    ILogger logger,
    IOptions<EmailTemplates> emailTemplates,
    ITableRepository<NotificationHistory> notificationHistoryRepository) : IConsumer<ManagementUserWelcomeEmailSent>
{
    private readonly ILogger _logger = logger;
    private readonly EmailTemplates _emailTemplates = emailTemplates.Value;
    private readonly ITableRepository<NotificationHistory> _notificationHistoryRepository = notificationHistoryRepository;

    public async Task Consume(ConsumeContext<ManagementUserWelcomeEmailSent> context)
    {
        _logger.Here().MethodEntered();
        _logger.Here()
            .ForContext("MessageId", context.MessageId)
            .WithCorrelationId(context.Message.CorrelationId)
            .Information("Message processing started for event {eventName}", nameof(ManagementUserWelcomeEmailSent));

        try
        {
            var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();
            var partitionKey = context.Message.Email;
            if (await _notificationHistoryRepository.ExistsAsync(partitionKey, messageId))
            {
                _logger.Here()
                    .WithCorrelationId(context.Message.CorrelationId)
                    .Information("Notification already recorded for message {messageId}", messageId);
                return;
            }

            NotificationHistory notification = new()
            {
                PartitionKey = partitionKey,
                RowKey = messageId,
                Subject = EmailSubjects.ManagementUserWelcomeEmailSent,
                Data = GetEmailData(context.Message),
                CorrelationId = context.Message.CorrelationId,
                IsPublished = false,
                TemplateName = _emailTemplates.ManagementUserWelcomeEmailSent,
                RecipientEmail = context.Message.Email,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _notificationHistoryRepository.AddAsync(notification);
            _logger.Here()
                .WithCorrelationId(context.Message.CorrelationId)
                .Information("Notification history table updated with new notification");
        }
        catch(Exception ex)
        {
            _logger.Here()
                .WithCorrelationId(context.Message.CorrelationId)
                .Error(ex.Message, "Error processing message for event {eventName}", nameof(ManagementUserWelcomeEmailSent));
            throw;
        }
        finally
        {
            _logger.Here().MethodExited();
        }
    }

    private string GetEmailData(ManagementUserWelcomeEmailSent message)
    {
        List<TemplateFields> fields =
        [
            new("[fullName]", message.FullName),
            new("[role]", message.Role),
        ];

        return JsonConvert.SerializeObject(fields);
    }
}
