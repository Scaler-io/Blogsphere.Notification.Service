using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blogsphere.Notification.Service.Configurations;
using Blogsphere.Notification.Service.Data;
using Blogsphere.Notification.Service.Entities;
using Blogsphere.Notification.Service.EventBus.Contracts;
using Blogsphere.Notification.Service.Extensions;
using Blogsphere.Notification.Service.Models.Constants;
using Blogsphere.Notification.Service.Models.Notification;
using Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Blogsphere.Notification.Service.EventBus.Consumers;

public class ManagementUserWelcomeEmailSentConsumer(
    ILogger logger,
    IOptions<EmailTemplates> emailTemplates,
    NotificationDbContext dbContext) : IConsumer<ManagementUserWelcomeEmailSent>
{
    private readonly ILogger _logger = logger;
    private readonly EmailTemplates _emailTemplates = emailTemplates.Value;
    private readonly NotificationDbContext _dbContext = dbContext;

    public async Task Consume(ConsumeContext<ManagementUserWelcomeEmailSent> context)
    {
        _logger.Here().MethodEntered();
        _logger.Here()
            .ForContext("MessageId", context.MessageId)
            .WithCorrelationId(context.Message.CorrelationId)
            .Information("Message processing started for event {eventName}", nameof(ManagementUserWelcomeEmailSent));

        try
        {
            NotificationHistory notification = new()
            {
                Subject = EmailSubjects.ManagementUserWelcomeEmailSent,
                Data = GetEmailData(context.Message),
                CorrelationId = context.Message.CorrelationId,
                IsPublished = false,
                TemplateName = _emailTemplates.ManagementUserWelcomeEmailSent,
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
                .Error(ex.Message, "Error processing message for event {eventName}", nameof(ManagementUserWelcomeEmailSent));
            throw;
        }
        finally
        {
            _logger.Here().MethodExited();
        }
    }

    private string GetEmailData(ManagementUserWelcomeEmailSent message)
    {
        List<TemplateFields> fields =
        [
            new("[fullName]", message.FullName),
            new("[role]", message.Role),
        ];

        return JsonConvert.SerializeObject(fields);
    }
}
