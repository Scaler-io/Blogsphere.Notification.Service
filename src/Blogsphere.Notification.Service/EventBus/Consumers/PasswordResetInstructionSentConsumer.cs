using Blogsphere.Notification.Service.Configurations;
using Blogsphere.Notification.Service.Data;
using Blogsphere.Notification.Service.Entities;
using Blogsphere.Notification.Service.Extensions;
using Blogsphere.Notification.Service.Models.Constants;
using Blogsphere.Notification.Service.Models.Notification;
using Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blogsphere.Notification.Service.EventBus.Consumers;
public class PasswordResetInstructionSentConsumer(ILogger logger, NotificationDbContext notificationDbContext,
    IConfiguration configuration, IOptions<EmailTemplates> emailTemplates) : IConsumer<PasswordResetInstructionSent>
{
    private readonly ILogger _logger = logger;
    private readonly NotificationDbContext _notificationDbContext = notificationDbContext;
    private readonly IConfiguration _configuration = configuration;
    private readonly EmailTemplates _emailTemplates = emailTemplates.Value;

    public async Task Consume(ConsumeContext<PasswordResetInstructionSent> context)
    {
        _logger.Here().MethodEntered();
        _logger.Here()
            .ForContext("MessageId", context.MessageId)
            .WithCorrelationId(context.Message.CorrelationId)
            .Information("Message processing started for event {eventName}", nameof(AuthCodeSent));

        try
        {
            var identityBaseUrl = _configuration["InfrastructureSettings:identityBaseUrl"];
            NotificationHistory notification = new()
            {
                Subject = EmailSubjects.PasswordResetInstructionSent,
                Data = GetEmailData(context.Message, identityBaseUrl),
                CorrelationId = context.Message.CorrelationId,  
                IsPublished = false,
                TemplateName = _emailTemplates.PasswordResetInstructionSent,
                RecipientEmail = context.Message.Email
            };
            _notificationDbContext.NotificationHistories.Add(notification);
            await _notificationDbContext.SaveChangesAsync();

            _logger.Here()
                .WithCorrelationId(context.Message.CorrelationId)
                .Information("Notification history table updated with new notification");

        }
        catch (Exception ex)
        {
            _logger.Here()
               .WithCorrelationId(context.Message.CorrelationId)
               .Error(ex.Message, "Error processing message for event {eventName}", nameof(UserInvitationSent));
            throw;
        }
        finally
        {
            _logger.Here().MethodExited();
        }
    }

    private string GetEmailData(PasswordResetInstructionSent message, string identityBaseUrl)
    {
        JObject data = JObject.Parse(message.AdditionalProperties.ToString());
        string token = data["token"].ToString();

        Uri url = new($"{identityBaseUrl}/account/resetpassword?email={message.Email}&token={token}");

        List<TemplateFields> fields =
        [
            new("[[email]]", message.Email),
            new("[[resetlink]]", url.ToString()),
        ];

        return JsonConvert.SerializeObject(fields);
    }
}
