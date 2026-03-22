using Blogsphere.Notification.Service.Configurations;
using Blogsphere.Notification.Service.Data.Storage;
using Blogsphere.Notification.Service.Entities;
using Blogsphere.Notification.Service.Extensions;
using Blogsphere.Notification.Service.Models.Constants;
using Blogsphere.Notification.Service.Models.Notification;
using Blogsphere.Notification.Service.Services.RateLimiting;
using Blogsphere.Notification.Service.Services.Sanitization;
using Microsoft.Extensions.Options;
using MimeKit;
using Newtonsoft.Json;

namespace Blogsphere.Notification.Service.Services;

public class EmailService(
    IOptions<EmailSettingOptions> emailSettings,
    ILogger logger,
    ISmtpClientFactory smtpClientFactory,
    ITableRepository<NotificationHistory> notificationHistoryRepository,
    IBlobRepository blobRepository,
    IRateLimiter<EmailRateLimit> rateLimiter,
    IHtmlSanitizer htmlSanitizer)
    : IEmailService
{
    private readonly EmailSettingOptions _emailSettings = emailSettings.Value;
    private readonly ILogger _logger = logger;
    private readonly ISmtpClientFactory _smtpClientFactory = smtpClientFactory;
    private readonly ITableRepository<NotificationHistory> _notificationHistoryRepository = notificationHistoryRepository;
    private readonly IBlobRepository _blobRepository = blobRepository;
    private readonly IRateLimiter<EmailRateLimit> _rateLimiter = rateLimiter;
    private readonly IHtmlSanitizer _htmlSanitizer = htmlSanitizer;

    public async Task SendEmailAsync()
    {
        var filter = $"IsPublished eq false and Channel eq '{NotificationChannels.Email}'";
        var notificationsToProcess = await _notificationHistoryRepository.QueryAsync(filter);

        if (notificationsToProcess == null || !notificationsToProcess.Any())
        {
            _logger.Here().Information("No notifications to process");
            return;
        }

        var mailClient = await _smtpClientFactory.CreateMailtrapClient();

        foreach (var notification in notificationsToProcess)
        {
            if (!_rateLimiter.TryAcquire())
            {
                _logger.Here().Warning("Rate limit reached, skipping remaining emails this cycle");
                break;
            }

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
                _logger.LogErrorSafely(ex, notification.CorrelationId, "Error sending email for subject {Subject}", notification.Subject);
            }
        }
    }

    private async Task<MimeMessage> ProcessMessage(NotificationHistory notification)
    {
        var emailTemplateText = await _blobRepository.GetBlobAsync("templates", $"{notification.TemplateName}.html");
        var emailFields = JsonConvert.DeserializeObject<List<TemplateFields>>(notification.Data);
        var builder = new BodyBuilder();

        var emailBuilder = new StringBuilder();
        using var reader = new StreamReader(emailTemplateText);
        emailBuilder.Append(await reader.ReadToEndAsync());

        foreach (var field in emailFields ?? [])
        {
            var sanitizedValue = _htmlSanitizer.Sanitize(field.Value);
            emailBuilder.Replace(field.Key, sanitizedValue);
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