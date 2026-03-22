using Blogsphere.Notification.Service.Configurations;
using Blogsphere.Notification.Service.Data.Storage;
using Blogsphere.Notification.Service.Entities;
using Blogsphere.Notification.Service.Extensions;
using Blogsphere.Notification.Service.Models.Constants;
using Blogsphere.Notification.Service.Models.Validation;
using Blogsphere.Notification.Service.Services.Validation;
using Contracts.Events;
using Microsoft.Extensions.Options;

namespace Blogsphere.Notification.Service.EventBus.Consumers;

public class PhoneVerificationCodeSentConsumer(
    ILogger logger,
    ITableRepository<NotificationHistory> notificationHistoryRepository,
    IOptions<EmailTemplates> emailTemplates,
    IValidationService validationService)
    : NotificationConsumerBase<PhoneVerificationCodeSent>(logger, notificationHistoryRepository, emailTemplates, validationService)
{
    protected override string GetEventName() => nameof(PhoneVerificationCodeSent);
    protected override string GetSubject() => nameof(PhoneVerificationCodeSent);
    protected override string GetTemplateName() => string.Empty;
    protected override string GetPartitionKey(PhoneVerificationCodeSent message) => message.PhoneNumber;
    protected override string GetCorrelationId(PhoneVerificationCodeSent message) => message.CorrelationId;
    protected override DateTimeOffset? GetCreatedAt(PhoneVerificationCodeSent message) =>
        message.CreatedOn == default ? DateTimeOffset.UtcNow : new DateTimeOffset(message.CreatedOn, TimeSpan.Zero);
    protected override string GetEmailData(PhoneVerificationCodeSent message) =>
        $"Your verification code is {message.Code}.";
    protected override string GetRecipientEmail(PhoneVerificationCodeSent message) => message.Email;
    protected override string GetRecipientPhone(PhoneVerificationCodeSent message) => message.PhoneNumber;
    protected override string GetChannel(PhoneVerificationCodeSent message) => NotificationChannels.Sms;
    protected override bool ValidatePayload(NotificationPayloadValidationContext ctx) =>
        ValidationService.ValidateSmsNotificationPayload(ctx);
}
