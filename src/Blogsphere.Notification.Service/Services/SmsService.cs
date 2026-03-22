using Blogsphere.Notification.Service.Configurations;
using Blogsphere.Notification.Service.Data.Storage;
using Blogsphere.Notification.Service.Entities;
using Blogsphere.Notification.Service.Extensions;
using Blogsphere.Notification.Service.Models.Constants;
using Microsoft.Extensions.Options;
using MimeKit;
using Blogsphere.Notification.Service.Services.RateLimiting;

namespace Blogsphere.Notification.Service.Services;

public class SmsService(
    IOptions<EmailSettingOptions> emailOptions,
    IOptions<SmsSettingOptions> smsOptions,
    ILogger logger,
    ISmtpClientFactory smtpClientFactory,
    ITableRepository<NotificationHistory> notificationHistoryRepository,
    IRateLimiter<SmsRateLimit> rateLimiter) : ISmsService
{
    private readonly EmailSettingOptions _emailSettings = emailOptions.Value;
    private readonly SmsSettingOptions _smsSettings = smsOptions.Value;
    private readonly ILogger _logger = logger;
    private readonly ISmtpClientFactory _smtpClientFactory = smtpClientFactory;
    private readonly ITableRepository<NotificationHistory> _notificationHistoryRepository = notificationHistoryRepository;
    private readonly IRateLimiter<SmsRateLimit> _rateLimiter = rateLimiter;

    public async Task SendSmsAsync()
    {
        var filter = $"IsPublished eq false and Channel eq '{NotificationChannels.Sms}'";
        var notificationsToProcess = await _notificationHistoryRepository.QueryAsync(filter);

        if (notificationsToProcess == null || !notificationsToProcess.Any())
        {
            _logger.Here().Information("No SMS notifications to process");
            return;
        }

        if (string.IsNullOrWhiteSpace(_smsSettings.TestInboxAddress))
        {
            _logger.Here().Warning("SmsSettings:TestInboxAddress is required");
            return;
        }

        var mailClient = await _smtpClientFactory.CreateMailtrapClient();

        foreach (var notification in notificationsToProcess)
        {
            if (!_rateLimiter.TryAcquire())
            {
                _logger.Here().Warning("Rate limit reached, skipping remaining SMS this cycle");
                break;
            }

            _logger.Here().Information("SMS processing {@subject}", notification.Subject);
            try
            {
                var mail = ProcessMessage(notification);
                await mailClient.SendAsync(mail);
                notification.IsPublished = true;
                notification.PublishTime = DateTimeOffset.UtcNow;
                notification.UpdatedAt = DateTimeOffset.UtcNow;
                await _notificationHistoryRepository.UpdateAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogErrorSafely(ex, notification.CorrelationId, "Error sending SMS for subject {Subject}", notification.Subject);
            }
        }
    }

    private MimeMessage ProcessMessage(NotificationHistory notification)
    {
        var mail = new MimeMessage();
        mail.To.Add(MailboxAddress.Parse(_smsSettings.TestInboxAddress));
        var from = string.IsNullOrWhiteSpace(_smsSettings.FromAddress)
            ? _emailSettings.CompanyAddress
            : _smsSettings.FromAddress;
        if (!string.IsNullOrWhiteSpace(from))
            mail.Sender = MailboxAddress.Parse(from);

        mail.Subject = $"SMS to {notification.RecipientPhone}";
        mail.Body = new TextPart("plain")
        {
            Text = $"To: {notification.RecipientPhone}\n{notification.Data}"
        };

        return mail;
    }
}
