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

public class ManagementUserWelcomeEmailSentConsumer(
    ILogger logger,
    IOptions<EmailTemplates> emailTemplates,
    ITableRepository<NotificationHistory> notificationHistoryRepository,
    IValidationService validationService)
    : NotificationConsumerBase<ManagementUserWelcomeEmailSent>(logger, notificationHistoryRepository, emailTemplates, validationService)
{
    protected override string GetEventName() => nameof(ManagementUserWelcomeEmailSent);
    protected override string GetSubject() => EmailSubjects.ManagementUserWelcomeEmailSent;
    protected override string GetTemplateName() => EmailTemplates.ManagementUserWelcomeEmailSent;
    protected override string GetPartitionKey(ManagementUserWelcomeEmailSent message) => message.Email;
    protected override string GetCorrelationId(ManagementUserWelcomeEmailSent message) => message.CorrelationId;
    protected override DateTimeOffset? GetCreatedAt(ManagementUserWelcomeEmailSent message) => DateTimeOffset.UtcNow;
    protected override string GetEmailData(ManagementUserWelcomeEmailSent message) =>
        JsonConvert.SerializeObject(new List<TemplateFields>
        {
            new("[fullName]", message.FullName),
            new("[role]", message.Role),
        });
}
