using Blogsphere.Notification.Service.Configurations;
using Blogsphere.Notification.Service.Data.Storage;
using Blogsphere.Notification.Service.Entities;
using Blogsphere.Notification.Service.Models.Constants;
using Blogsphere.Notification.Service.Models.Notification;
using Blogsphere.Notification.Service.Services.Validation;
using Contracts.Events;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Blogsphere.Notification.Service.EventBus.Consumers;

public class PasswordResetOneTimeCodeSentConsumer(
    ILogger logger,
    ITableRepository<NotificationHistory> notificationHistoryRepository,
    IOptions<EmailTemplates> emailTemplates,
    IValidationService validationService)
    : NotificationConsumerBase<PasswordResetOneTimeCodeSent>(logger, notificationHistoryRepository, emailTemplates, validationService)
{
    protected override string GetEventName() => nameof(PasswordResetOneTimeCodeSent);
    protected override string GetSubject() => EmailSubjects.PasswordResetOneTimeCode;
    protected override string GetTemplateName() => EmailTemplates.PasswordResetOneTimeCodeSent;
    protected override string GetPartitionKey(PasswordResetOneTimeCodeSent message) => message.Email;
    protected override string GetCorrelationId(PasswordResetOneTimeCodeSent message) => message.CorrelationId;
    protected override DateTimeOffset? GetCreatedAt(PasswordResetOneTimeCodeSent message) => DateTimeOffset.UtcNow;
    protected override string GetEmailData(PasswordResetOneTimeCodeSent message) =>
        JsonConvert.SerializeObject(new List<TemplateFields>
        {
            new("[Email]", message.Email),
            new("[Code]", message.Code),
        });
}
