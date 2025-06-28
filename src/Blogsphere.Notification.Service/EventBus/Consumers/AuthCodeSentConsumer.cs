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

public class AuthCodeSentConsumer(ILogger logger, NotificationDbContext dbContext, IOptions<EmailTemplates> emailTemplates) : IConsumer<AuthCodeSent>
{
    private readonly ILogger _logger = logger;
    private readonly EmailTemplates _emailTemplates = emailTemplates.Value;
    private readonly NotificationDbContext _dbContext = dbContext;

    public async Task Consume(ConsumeContext<AuthCodeSent> context)
    {
         _logger.Here().MethodEntered();
        _logger.Here()
            .ForContext("MessageId", context.MessageId)
            .WithCorrelationId(context.Message.CorrelationId)
            .Information("Message processing started for event {eventName}", nameof(AuthCodeSent));

        try{
            
            NotificationHistory notification = new()
            {
                Subject = EmailSubjects.UserInvitation,
                Data = GetEmailData(context.Message),
                CorrelationId = context.Message.CorrelationId,
                IsPublished = false,
                TemplateName = _emailTemplates.AuthCodeSent,
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
        }
        finally
        {
            _logger.Here().MethodExited();
        }
    }

    private string GetEmailData(AuthCodeSent message)
    {
        List<TemplateFields> fields =
        [
            new("[Email]", message.Email),
            new("[VerificationCode]", message.Code),
        ];

        return JsonConvert.SerializeObject(fields);
    }
}
