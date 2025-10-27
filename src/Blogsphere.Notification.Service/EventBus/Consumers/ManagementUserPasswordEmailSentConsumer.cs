using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

namespace Blogsphere.Notification.Service.EventBus.Consumers;

public class ManagementUserPasswordEmailSentConsumer(ILogger logger, IOptions<EmailTemplates> emailTemplates, NotificationDbContext notificationDbContext) : IConsumer<ManagementUserPasswordEmailSent>
{
    private readonly ILogger _logger = logger;
    private readonly EmailTemplates _emailTemplates = emailTemplates.Value;
    private readonly NotificationDbContext _notificationDbContext = notificationDbContext;

    public async Task Consume(ConsumeContext<ManagementUserPasswordEmailSent> context)
    {
        _logger.Here().MethodEntered();
        _logger.Here()
            .ForContext("MessageId", context.MessageId)
            .WithCorrelationId(context.Message.CorrelationId)
            .Information("Message processing started for event {eventName}", nameof(ManagementUserPasswordEmailSent));

        try
        {
            NotificationHistory notification = new()
            {
                Subject = EmailSubjects.ManagementUserPasswordEmailSent,
                Data = GetEmailData(context.Message),
                CorrelationId = context.Message.CorrelationId,
                IsPublished = false,
                TemplateName = _emailTemplates.ManagementUserPasswordEmailSent,
                RecipientEmail = context.Message.Email
            };
            _notificationDbContext.NotificationHistories.Add(notification);
            await _notificationDbContext.SaveChangesAsync();

            _logger.Here()
                .WithCorrelationId(context.Message.CorrelationId)
                .Information("Notification history table updated with new notification");
        }
        catch(Exception ex)
        {
            _logger.Here()
                .WithCorrelationId(context.Message.CorrelationId)
                .Error(ex.Message, "Error processing message for event {eventName}", nameof(ManagementUserPasswordEmailSent));
            throw;
        }
        finally
        {
            _logger.Here().MethodExited();
        }
    }

    private string GetEmailData(ManagementUserPasswordEmailSent message)
    {
        List<TemplateFields> fields =
        [
            new("[fullName]", message.FullName),
            new("[email]", message.Email),
            new("[password]", message.Password),
        ];

        return JsonConvert.SerializeObject(fields);
    }
}
