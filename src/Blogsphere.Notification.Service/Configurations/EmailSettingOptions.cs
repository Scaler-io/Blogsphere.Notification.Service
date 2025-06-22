using Destructurama.Attributed;

namespace Blogsphere.Notification.Service.Configurations;

public sealed class EmailSettingOptions
{
    public const string OptionName = "EmailSettings";
    public string Server { get; set; }
    public int Port { get; set; }
    public string CompanyAddress { get; set; }
    [LogMasked]
    public string UserName { get; set; }
    [LogMasked]
    public string Password { get; set; }
}