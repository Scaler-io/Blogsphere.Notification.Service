namespace Blogsphere.Notification.Service.Configurations;

public sealed class ErrorQueueReprocessorOption
{
    public const string OptionName = "ErrorQueueReprocessor";
    public bool Enabled { get; set; } = true;
    public int PollIntervalSeconds { get; set; } = 30;
    public int MaxAttempts { get; set; } = 5;
    public List<string> ErrorQueues { get; set; } = [];
}
