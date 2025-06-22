using Blogsphere.Notification.Service.Configurations;
using Blogsphere.Notification.Service.Data;
using Blogsphere.Notification.Service.Entities;
using Blogsphere.Notification.Service.Extensions;
using Blogsphere.Notification.Service.Models.Constants;
using Blogsphere.Notification.Service.Models.Notification;
using Blogsphere.Notification.Service.Services;
using Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blogsphere.Notification.Service.EventBus.Consumers;

public class UserInvitationSentConsumer(ILogger logger, IOptions<EmailTemplates> emailTemplates, NotificationDbContext dbContext, IConfiguration configuration) : IConsumer<UserInvitationSent>
{
    private readonly ILogger _logger = logger;
    private readonly EmailTemplates _emailTemplates = emailTemplates.Value;
    private readonly NotificationDbContext _dbContext = dbContext;
    private readonly IConfiguration _configuration = configuration;

    public async Task Consume(ConsumeContext<UserInvitationSent> context)
    {
        _logger.Here().MethodEntered();
        _logger.Here()
            .ForContext("MessageId", context.MessageId)
            .WithCorrelationId(context.Message.CorrelationId)
            .Information("Message processing started for event {eventName}", nameof(UserInvitationSent));

        try
        {
            var identityBaseUrl = _configuration["InfrastructureSettings:identityBaseUrl"];
            NotificationHistory notification = new()
            {
                Subject = EmailSubjects.UserInvitation,
                Data = GetEmailData(context.Message, identityBaseUrl),
                CorrelationId = context.Message.CorrelationId,
                IsPublished = false,
                TemplateName = _emailTemplates.UserInvite,
                RecipientEmail = context.Message.Email
            };

            _dbContext.NotificationHistories.Add(notification);
            await _dbContext.SaveChangesAsync();

            _logger.Here()
                .WithCorrelationId(context.Message.CorrelationId)
                .Information("Notification history table updated with new notification");
        }
        catch(Exception ex)
        {
            _logger.Here()
                .WithCorrelationId(context.Message.CorrelationId)
                .Error(ex.Message, "Error processing message for event {eventName}", nameof(UserInvitationSent));
            throw;
        }finally
        {
            _logger.Here().MethodExited();
        }
    }

    private string GetEmailData(UserInvitationSent message, string identityBaseUrl)
    {
        JObject data = JObject.Parse(message.AdditionalProperties.ToString());
        string token = data["token"].ToString();

        Uri url = new($"{identityBaseUrl}/account/emailverification?userId={message.UserId}&token={token}");
        List<TemplateFields> fields = new()
        {
            new("[firstname]", message.FirstName),
            new("[lastname]", message.LastName),
            new("[acceptanceLink]", url.ToString())
        };

        return JsonConvert.SerializeObject(fields);
    }
}