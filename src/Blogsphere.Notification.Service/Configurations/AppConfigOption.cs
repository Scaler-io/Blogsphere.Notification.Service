namespace Blogsphere.Notification.Service.Configurations;

public sealed class AppConfigOption
{
    public const string OptionName = "AppConfigurations";
    public string ApplicationIdentifier { get; set; }
    public string ApplicationEnvironment { get; set; }
    public int NotificationProcessInterval { get; set; }
    public string IntervalUnit { get; set; }
}