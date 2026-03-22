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

public class ManagementUserPasswordEmailSentConsumer(
    ILogger logger,
    IOptions<EmailTemplates> emailTemplates,
    ITableRepository<NotificationHistory> notificationHistoryRepository,
    IValidationService validationService)
    : NotificationConsumerBase<ManagementUserPasswordEmailSent>(logger, notificationHistoryRepository, emailTemplates, validationService)
{
    protected override string GetEventName() => nameof(ManagementUserPasswordEmailSent);
    protected override string GetSubject() => EmailSubjects.ManagementUserPasswordEmailSent;
    protected override string GetTemplateName() => EmailTemplates.ManagementUserPasswordEmailSent;
    protected override string GetPartitionKey(ManagementUserPasswordEmailSent message) => message.Email;
    protected override string GetCorrelationId(ManagementUserPasswordEmailSent message) => message.CorrelationId;
    protected override DateTimeOffset? GetCreatedAt(ManagementUserPasswordEmailSent message) => DateTimeOffset.UtcNow;
    protected override string GetEmailData(ManagementUserPasswordEmailSent message) =>
        JsonConvert.SerializeObject(new List<TemplateFields>
        {
            new("[fullName]", message.FullName),
            new("[email]", message.Email),
            new("[password]", message.Password),
        });
}
