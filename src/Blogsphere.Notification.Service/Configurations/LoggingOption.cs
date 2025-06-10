namespace Blogsphere.Notification.Service.Configurations;

public sealed class LoggingOption
{
    public const string OptionName = "Logging";
    public bool IncludeScopes { get; set; }
    public string LogOutputTemplate { get; set; }
    public Console Console { get; set; }
    public Elastic Elastic { get; set; }
}

public sealed class Console
{
    public bool Enabled { get; set; }
    public string LogLevel { get; set; }
}

public sealed class Elastic
{
    public bool Enabled { get; set; }
    public string LogLevel { get; set; }
}