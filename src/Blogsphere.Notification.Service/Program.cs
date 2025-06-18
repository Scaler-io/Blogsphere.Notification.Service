
using Blogsphere.Notification.Service;
using Blogsphere.Notification.Service.BackgroundJobs;
using Blogsphere.Notification.Service.Data;
using Blogsphere.Notification.Service.DI;
using Microsoft.EntityFrameworkCore;

ILogger logger = null;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) => {
        
        var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

        services.AddSingleton<IConfiguration>(configuration);

        var serviceProvider = services.BuildServiceProvider();

        logger = Logging.GetLogger(configuration);
        services.AddSingleton(logger);
        
        services.ConfigureServices(configuration)
        .ConfigureOptions(configuration)
        .ConfigureDataSevices(configuration);

        services.AddHostedService<EventBusStarterJob>();
        services.AddHostedService<EmailProcessingJob>();
        
    }).UseSerilog(logger)
    .Build();

using var scope = host.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

try
{
    await dbContext.Database.MigrateAsync();
    await host.RunAsync(); 
}
finally
{
    Log.CloseAndFlush();
}

