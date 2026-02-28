
using Blogsphere.Notification.Service;
using Blogsphere.Notification.Service.BackgroundJobs;
using Blogsphere.Notification.Service.DI;

ILogger logger = null;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {

        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        logger = Logging.GetLogger(configuration);
        services.AddSingleton(logger);

        services.ConfigurationSettings()
        .ConfigureServices(configuration)
        .ConfigureOptions(configuration)
        .ConfigureDataServices(configuration);

        services.AddHostedService<EventBusStarterJob>();
        services.AddHostedService<EmailProcessingJob>();
        services.AddHostedService<ErrorQueueReprocessorJob>();

    }).UseSerilog(logger)
    .Build();

try
{
    await host.RunAsync();
}
finally
{
    Log.CloseAndFlush();
}

