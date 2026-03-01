using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blogsphere.Notification.Service.Configurations;
using Blogsphere.Notification.Service.Data.Storage;
using Blogsphere.Notification.Service.Entities;
using Blogsphere.Notification.Service.Extensions;
using Contracts.Events;
using Microsoft.Extensions.Logging;
using MassTransit;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Blogsphere.Notification.Service.Models.Notification;
using Blogsphere.Notification.Service.Models.Constants;

namespace Blogsphere.Notification.Service.EventBus.Consumers;

public class PasswordResetOneTimeCodeSentConsumer(ILogger logger, ITableRepository<NotificationHistory> notificationHistoryRepository, IOptions<EmailTemplates> emailTemplates) : IConsumer<PasswordResetOneTimeCodeSent>
{
    private readonly ILogger _logger = logger;
    private readonly EmailTemplates _emailTemplates = emailTemplates.Value;
    private readonly ITableRepository<NotificationHistory> _notificationHistoryRepository = notificationHistoryRepository;

    public async Task Consume(ConsumeContext<PasswordResetOneTimeCodeSent> context)
    {
        _logger.Here().MethodEntered();
        _logger.Here()
            .ForContext("MessageId", context.MessageId)
            .WithCorrelationId(context.Message.CorrelationId)
            .Information("Message processing started for event {eventName}", nameof(PasswordResetOneTimeCodeSent));

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
                Subject = EmailSubjects.PasswordResetOneTimeCode,
                Data = GetEmailData(context.Message),
                CorrelationId = context.Message.CorrelationId,
                IsPublished = false,
                TemplateName = _emailTemplates.PasswordResetOneTimeCodeSent,
                RecipientEmail = context.Message.Email,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            await _notificationHistoryRepository.AddAsync(notification);
            _logger.Here()
                .WithCorrelationId(context.Message.CorrelationId)
                .Information("Notification history table updated with new notification");
        }
        catch (Exception ex)
        {
            _logger.Here()
                .WithCorrelationId(context.Message.CorrelationId)
                .Error(ex.Message, "Error processing message for event {eventName}", nameof(PasswordResetOneTimeCodeSent));
            throw;
        }
        finally
        {
            _logger.Here().MethodExited();
        }
    }

    private string GetEmailData(PasswordResetOneTimeCodeSent message)
    {
        List<TemplateFields> fields =
        [
            new("[Email]", message.Email),
            new("[Code]", message.Code),
        ];

        return JsonConvert.SerializeObject(fields);
    }
}
