using Blogsphere.Notification.Service.Configurations;
using Blogsphere.Notification.Service.Extensions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;

namespace Blogsphere.Notification.Service.Services;

public sealed class SmtpClientFactory(IOptions<EmailSettingOptions> emailOptions, ILogger logger) : ISmtpClientFactory
{
    private readonly EmailSettingOptions _emailSettings = emailOptions.Value;
    private readonly ILogger _logger = logger;

    public async Task<SmtpClient> CreateMailtrapClient(CancellationToken cancellationToken = default)
    {
        var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_emailSettings.Server, _emailSettings.Port, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(_emailSettings.UserName, _emailSettings.Password, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogErrorSafely(ex, null, "Failed to establish connection to SMTP server");
        }

        _logger.Here().Information("Mail client established");
        return client;
    }
}
