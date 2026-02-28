
using Blogsphere.Notification.Service.Configurations;
using Blogsphere.Notification.Service.Data.Storage;
using Blogsphere.Notification.Service.Entities;
using Blogsphere.Notification.Service.Extensions;
using Blogsphere.Notification.Service.Models.Notification;
using Microsoft.Extensions.Options;
using MimeKit;
using Newtonsoft.Json;

namespace Blogsphere.Notification.Service.Services;

public class EmailService(
    IOptions<EmailSettingOptions> emailSettings,
    ILogger logger,
    ITableRepository<NotificationHistory> notificationHistoryRepository,
    IBlobRepository blobRepository)
    : EmailServiceBase(emailSettings.Value, logger), IEmailService
{
    private readonly ITableRepository<NotificationHistory> _notificationHistoryRepository = notificationHistoryRepository;
    private readonly IBlobRepository _blobRepository = blobRepository;

    public async Task SendEmailAsync()
    {
        var filter = "IsPublished eq false";
        var notificationsToProcess = await _notificationHistoryRepository.QueryAsync(filter);

        if (notificationsToProcess == null || !notificationsToProcess.Any())
        {
            _logger.Here().Information("No notifications to process");
            return;
        }

        var mailClient = await CreateMailClient();

        foreach (var notification in notificationsToProcess)
        {
            _logger.Here().Information("Message processing {@subject}", notification.Subject);
            try
            {
                var mail = await ProcessMessage(notification);
                await mailClient.SendAsync(mail);
                notification.IsPublished = true;
                notification.PublishTime = DateTimeOffset.UtcNow;
                notification.UpdatedAt = DateTimeOffset.UtcNow;
                await _notificationHistoryRepository.UpdateAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.Here().Error(ex, "Error sending email {@subject}", notification.Subject);
            }
        }
    }

    protected override async Task<MimeMessage> ProcessMessage(NotificationHistory notification)
    {
        var emailTempateText = await _blobRepository.GetBlobAsync("templates", $"{notification.TemplateName}.html");
        var emailFields = JsonConvert.DeserializeObject<List<TemplateFields>>(notification.Data);
        var builder = new BodyBuilder();

        var emailBuilder = new StringBuilder();
        using var reader = new StreamReader(emailTempateText);
        emailBuilder.Append(await reader.ReadToEndAsync());

        foreach (var field in emailFields)
        {
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