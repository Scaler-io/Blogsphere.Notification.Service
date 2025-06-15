using Blogsphere.Notification.Service.Configurations;
using Destructurama;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;

namespace Blogsphere.Notification.Service;

public sealed class Logging
{
    public static ILogger GetLogger(IConfiguration configuration)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var loggingOptions = configuration.GetSection(LoggingOption.OptionName).Get<LoggingOption>();
        var appConfigOption = configuration.GetSection(AppConfigOption.OptionName).Get<AppConfigOption>();
        var elasticOption = configuration.GetSection(ElasticSearchOption.OptionName).Get<ElasticSearchOption>();

        var logIndexPattern = $"Blogsphere.Notification.Service-{environment?.ToLower().Replace(".", "-")}";

        Enum.TryParse(loggingOptions.Console.LogLevel, false, out LogEventLevel minimumConsoleEventLevel);
        Enum.TryParse(loggingOptions.Elastic.LogLevel, false, out LogEventLevel minimumElasticEventLevel);

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty(nameof(Environment.MachineName), Environment.MachineName)
            .Enrich.WithProperty(nameof(appConfigOption.ApplicationIdentifier), appConfigOption.ApplicationIdentifier)
            .Enrich.WithProperty(nameof(appConfigOption.ApplicationEnvironment), appConfigOption.ApplicationEnvironment);

        if(loggingOptions.Console.Enabled)
        {
            loggerConfiguration.WriteTo.Console(
                restrictedToMinimumLevel: minimumConsoleEventLevel,
                outputTemplate: loggingOptions.LogOutputTemplate,
                theme: AnsiConsoleTheme.Code
            );
        }

        if(loggingOptions.Elastic.Enabled)
        {
            loggerConfiguration.WriteTo.Elasticsearch(
                nodeUris: elasticOption.Uri, 
                indexFormat: logIndexPattern, 
                restrictedToMinimumLevel: minimumElasticEventLevel
            );
        }
        
        return loggerConfiguration
            .Destructure
            .UsingAttributes()
            .CreateLogger();
    }
}