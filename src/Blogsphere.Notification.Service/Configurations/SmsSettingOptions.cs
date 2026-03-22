namespace Blogsphere.Notification.Service.Configurations;

public sealed class SmsSettingOptions
{
    public const string OptionName = "SmsSettings";
    public string TestInboxAddress { get; set; }
    public string FromAddress { get; set; }
}
