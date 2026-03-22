using Blogsphere.Notification.Service.Configurations;
using Blogsphere.Notification.Service.Data.Storage;
using Blogsphere.Notification.Service.Entities;
using Blogsphere.Notification.Service.Models.Constants;
using Blogsphere.Notification.Service.Models.Notification;
using Blogsphere.Notification.Service.Services.Validation;
using Contracts.Events;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blogsphere.Notification.Service.EventBus.Consumers;

public class AuthCodeSentConsumer(
    ILogger logger,
    ITableRepository<NotificationHistory> notificationHistoryRepository,
    IOptions<EmailTemplates> emailTemplates,
    IValidationService validationService)
    : NotificationConsumerBase<AuthCodeSent>(logger, notificationHistoryRepository, emailTemplates, validationService)
{
    protected override string GetEventName() => nameof(AuthCodeSent);
    protected override string GetSubject() => EmailSubjects.AuthCodeSent;
    protected override string GetTemplateName() => EmailTemplates.AuthCodeSent;
    protected override string GetPartitionKey(AuthCodeSent message) => message.Email;
    protected override string GetCorrelationId(AuthCodeSent message) => message.CorrelationId;
    protected override DateTimeOffset? GetCreatedAt(AuthCodeSent message) =>
        message.CreatedOn == default ? DateTimeOffset.UtcNow : new DateTimeOffset(message.CreatedOn, TimeSpan.Zero);
    protected override string GetEmailData(AuthCodeSent message)
    {
        var purpose = JObject.Parse(message.AdditionalProperties.ToString())["purpose"].ToString();
        var disable2FABody = "We received a request to disable two-factor authentication (2FA) for your Blogsphere account. To complete the process, please use the verification code below:";

        var signInBody = "We received a request to sign in to your Blogsphere account. To complete the sign-in process, please use the verification code below:";
        return JsonConvert.SerializeObject(new List<TemplateFields>
        {
            new("[Email]", message.Email),
            new("[VerificationCode]", message.Code),
            new("[Message]", purpose == "Disable2FA" ? disable2FABody : signInBody )
        });
    }

}
