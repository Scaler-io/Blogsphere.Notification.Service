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

public class PasswordResetInstructionSentConsumer(
    ILogger logger,
    ITableRepository<NotificationHistory> notificationHistoryRepository,
    IConfiguration configuration,
    IOptions<EmailTemplates> emailTemplates,
    IValidationService validationService)
    : NotificationConsumerBase<PasswordResetInstructionSent>(logger, notificationHistoryRepository, emailTemplates, validationService, configuration)
{
    protected override string GetEventName() => nameof(PasswordResetInstructionSent);
    protected override string GetSubject() => EmailSubjects.PasswordResetInstructionSent;
    protected override string GetTemplateName() => EmailTemplates.PasswordResetInstructionSent;
    protected override string GetPartitionKey(PasswordResetInstructionSent message) => message.Email;
    protected override string GetCorrelationId(PasswordResetInstructionSent message) => message.CorrelationId;
    protected override DateTimeOffset? GetCreatedAt(PasswordResetInstructionSent message) => DateTimeOffset.UtcNow;
    protected override string GetEmailData(PasswordResetInstructionSent message)
    {
        var identityBaseUrl = GetIdentityBaseUrl();
        var data = JObject.Parse(message.AdditionalProperties.ToString());
        var token = data["token"].ToString();
        var url = new Uri($"{identityBaseUrl}/account/resetpassword?email={message.Email}&token={token}");
        return JsonConvert.SerializeObject(new List<TemplateFields>
        {
            new("[[email]]", message.Email),
            new("[[resetlink]]", url.ToString()),
        });
    }
}
