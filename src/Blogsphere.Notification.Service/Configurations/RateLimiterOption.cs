namespace Blogsphere.Notification.Service.Configurations;

public sealed class RateLimiterOption
{
    public const string OptionName = "RateLimiter";
    public bool Enabled { get; set; } = true;
    public int MaxEmailsPerMinute { get; set; } = 60;
    public int MaxSmsPerMinute { get; set; } = 60;
}
