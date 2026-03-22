using Blogsphere.Notification.Service.Configurations;
using Blogsphere.Notification.Service.Data.Storage;
using Blogsphere.Notification.Service.Entities;
using Blogsphere.Notification.Service.Extensions;
using Blogsphere.Notification.Service.Models.Constants;
using Blogsphere.Notification.Service.Models.Validation;
using Blogsphere.Notification.Service.Services.Validation;
using MassTransit;
using Microsoft.Extensions.Options;

namespace Blogsphere.Notification.Service.EventBus.Consumers;

public abstract class NotificationConsumerBase<TMessage> : IConsumer<TMessage>
    where TMessage : class
{
    protected readonly ILogger Logger;
    protected readonly ITableRepository<NotificationHistory> NotificationHistoryRepository;
    protected readonly EmailTemplates EmailTemplates;
    protected readonly IValidationService ValidationService;
    protected readonly IConfiguration Configuration;

    protected NotificationConsumerBase(
        ILogger logger,
        ITableRepository<NotificationHistory> notificationHistoryRepository,
        IOptions<EmailTemplates> emailTemplates,
        IValidationService validationService,
        IConfiguration configuration = null)
    {
        Logger = logger;
        NotificationHistoryRepository = notificationHistoryRepository;
        EmailTemplates = emailTemplates.Value;
        ValidationService = validationService;
        Configuration = configuration;
    }

    protected string GetIdentityBaseUrl() => Configuration?["InfrastructureSettings:identityBaseUrl"] ?? string.Empty;

    public async Task Consume(ConsumeContext<TMessage> context)
    {
        Logger.Here().MethodEntered();
        Logger.Here()
            .ForContext("MessageId", context.MessageId)
            .WithCorrelationId(GetCorrelationId(context.Message))
            .Information("Message processing started for event {eventName}", GetEventName());

        try
        {
            var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();
            var partitionKey = GetPartitionKey(context.Message);
            var data = GetNotificationData(context.Message);

            if (!ValidatePayload(new NotificationPayloadValidationContext(partitionKey, messageId, data.Length, GetEventName(), Logger, GetCorrelationId(context.Message))))
                return;

            if (await NotificationHistoryRepository.ExistsAsync(partitionKey, messageId))
            {
                Logger.Here()
                    .WithCorrelationId(GetCorrelationId(context.Message))
                    .Information("Notification already recorded for message {messageId}", messageId);
                return;
            }

            var notification = CreateNotification(partitionKey, messageId, data, context.Message);
            await NotificationHistoryRepository.AddAsync(notification);

            Logger.Here()
                .WithCorrelationId(GetCorrelationId(context.Message))
                .Information("Notification history table updated with new notification");
        }
        catch (Exception ex)
        {
            Logger.LogErrorSafely(ex, GetCorrelationId(context.Message), "Error processing message for event {EventName}", GetEventName());
            throw;
        }
        finally
        {
            Logger.Here().MethodExited();
        }
    }

    private NotificationHistory CreateNotification(string partitionKey, string messageId, string data, TMessage message)
    {
        return new NotificationHistory
        {
            PartitionKey = partitionKey,
            RowKey = messageId,
            Subject = GetSubject(),
            Data = data,
            CorrelationId = GetCorrelationId(message),
            IsPublished = false,
            TemplateName = GetTemplateName(),
            RecipientEmail = GetRecipientEmail(message),
            RecipientPhone = GetRecipientPhone(message),
            Channel = GetChannel(message),
            CreatedAt = GetCreatedAt(message),
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    protected abstract string GetEventName();
    protected abstract string GetSubject();
    protected abstract string GetTemplateName();
    protected abstract string GetPartitionKey(TMessage message);
    protected abstract string GetCorrelationId(TMessage message);
    protected abstract DateTimeOffset? GetCreatedAt(TMessage message);
    protected abstract string GetEmailData(TMessage message);
    protected virtual string GetNotificationData(TMessage message) => GetEmailData(message);
    protected virtual string GetRecipientEmail(TMessage message) => GetPartitionKey(message);
    protected virtual string GetRecipientPhone(TMessage message) => string.Empty;
    protected virtual string GetChannel(TMessage message) => NotificationChannels.Email;
    protected virtual bool ValidatePayload(NotificationPayloadValidationContext ctx) =>
        ValidationService.ValidateEmailNotificationPayload(ctx);
}
