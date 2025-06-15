using Blogsphere.Notification.Service.Configurations;
using Blogsphere.Notification.Service.Entities;
using Blogsphere.Notification.Service.Extensions;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Blogsphere.Notification.Service.Services;

public abstract class EmailServiceBase(EmailSettingOptions emailSettings, ILogger logger)
{
    protected readonly EmailSettingOptions _emailSettings = emailSettings;
    protected readonly ILogger _logger = logger;

    protected async Task<SmtpClient> CreateMailClient()
    {
        var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_emailSettings.Server, _emailSettings.Port, options: SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(userName: _emailSettings.UserName, password: _emailSettings.Password);
        }   
        catch(Exception ex)
        {
             _logger.Here().Information("Failed to establish connection to SMTP server. {@stackTrace}", ex);
        }

        _logger.Here().Information("Mail client established");
        return client;
    }

    protected abstract Task<MimeMessage> ProcessMessage(NotificationHistory notification);
}