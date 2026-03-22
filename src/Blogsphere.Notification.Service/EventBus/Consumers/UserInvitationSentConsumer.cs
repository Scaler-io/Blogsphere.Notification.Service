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

public class UserInvitationSentConsumer(
    ILogger logger,
    IOptions<EmailTemplates> emailTemplates,
    ITableRepository<NotificationHistory> notificationHistoryRepository,
    IConfiguration configuration,
    IValidationService validationService)
    : NotificationConsumerBase<UserInvitationSent>(logger, notificationHistoryRepository, emailTemplates, validationService, configuration)
{
    protected override string GetEventName() => nameof(UserInvitationSent);
    protected override string GetSubject() => EmailSubjects.UserInvitation;
    protected override string GetTemplateName() => EmailTemplates.UserInvite;
    protected override string GetPartitionKey(UserInvitationSent message) => message.Email;
    protected override string GetCorrelationId(UserInvitationSent message) => message.CorrelationId;
    protected override DateTimeOffset? GetCreatedAt(UserInvitationSent message) => DateTimeOffset.UtcNow;
    protected override string GetEmailData(UserInvitationSent message)
    {
        var identityBaseUrl = GetIdentityBaseUrl();
        var data = JObject.Parse(message.AdditionalProperties.ToString());
        var token = data["token"].ToString();
        var url = new Uri($"{identityBaseUrl}/account/emailverification?userId={message.UserId}&token={token}");
        return JsonConvert.SerializeObject(new List<TemplateFields>
        {
            new("[firstname]", message.FirstName),
            new("[lastname]", message.LastName),
            new("[acceptanceLink]", url.ToString()),
        });
    }
}
