using MailKit.Net.Smtp;

namespace Blogsphere.Notification.Service.Services;

public interface ISmtpClientFactory
{
    Task<SmtpClient> CreateMailtrapClient(CancellationToken cancellationToken = default);
}
