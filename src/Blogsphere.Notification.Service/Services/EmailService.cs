
using Blogsphere.Notification.Service.Configurations;
using Blogsphere.Notification.Service.Data;
using Blogsphere.Notification.Service.Data.Storage;
using Blogsphere.Notification.Service.Entities;
using Blogsphere.Notification.Service.Extensions;
using Blogsphere.Notification.Service.Models.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using Newtonsoft.Json;

namespace Blogsphere.Notification.Service.Services;

public class EmailService(IOptions<EmailSettingOptions> emailSettings, ILogger logger, NotificationDbContext dbContext, IBlobStorageService blobStorageService) 
    : EmailServiceBase(emailSettings.Value, logger), IEmailService 
{
    private readonly NotificationDbContext _dbContext = dbContext; 
    private readonly IBlobStorageService _blobStorageService = blobStorageService;

    public async Task SendEmailAsync()
    {
        var notificationsToProcess = await _dbContext.NotificationHistories
            .Where(x => !x.IsPublished)
            .ToListAsync();

            if(notificationsToProcess == null || notificationsToProcess.Count == 0)
        {
            _logger.Here().Information("No notifications to process");
            return;
        }

        var mailClient = await CreateMailClient();

        foreach(var notification in notificationsToProcess)
        {
            _logger.Here().Information("Message processing {@subject}", notification.Subject);
            var mail = await ProcessMessage(notification);
            try
            {
                await mailClient.SendAsync(mail);
                notification.IsPublished = true;
                notification.PublishTime = DateTime.UtcNow;
                _dbContext.NotificationHistories.Update(notification);
                await _dbContext.SaveChangesAsync();   
            }
            catch(Exception ex)
            {
                _logger.Here().Error(ex, "Error sending email {@subject}", notification.Subject);
            }
        }
    }

    protected override async Task<MimeMessage> ProcessMessage(NotificationHistory notification)
    {
        var emailTempateText = await _blobStorageService.GetBlobAsync($"templates/{notification.TemplateName}.html");
        var emailFields = JsonConvert.DeserializeObject<List<TemplateFields>>(notification.Data);
        var builder = new BodyBuilder();

        var emailBuilder = new StringBuilder();
        using var reader = new StreamReader(emailTempateText);
        emailBuilder.Append(await reader.ReadToEndAsync());
        
        foreach(var field in emailFields)
        {
            _logger.Here().Information("field {0}: {1}", field.Key, field.Value);
            emailBuilder.Replace(field.Key, field.Value);
        }

        var email = new MimeMessage();
        email.To.Add(MailboxAddress.Parse(notification.RecipientEmail));
        email.Subject = notification.Subject;
        email.Sender = MailboxAddress.Parse(_emailSettings.CompanyAddress);
        builder.HtmlBody = emailBuilder.ToString();
        email.Body = builder.ToMessageBody();

        return email;
    }
}