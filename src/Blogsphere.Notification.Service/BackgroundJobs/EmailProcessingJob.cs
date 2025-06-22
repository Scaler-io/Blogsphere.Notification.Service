
using Blogsphere.Notification.Service.Configurations;
using Blogsphere.Notification.Service.Extensions;
using Blogsphere.Notification.Service.Services;
using Microsoft.Extensions.Options;

namespace Blogsphere.Notification.Service.BackgroundJobs;

public class EmailProcessingJob(IOptions<AppConfigOption> appConfigOption, IServiceProvider serviceProvider) : BackgroundService
{
    private readonly AppConfigOption _appConfigOption = appConfigOption.Value;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            System.Console.WriteLine($"Next job running {DateTime.Now.AddSeconds(10)}");
            using var scope = _serviceProvider.CreateScope();

            IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            await emailService.SendEmailAsync();

            await Task.Delay(_appConfigOption.IntervalUnit == "ss"
            ? TimeSpan.FromSeconds(_appConfigOption.NotificationProcessInterval)
            : TimeSpan.FromMinutes(_appConfigOption.NotificationProcessInterval), stoppingToken);
        }
    }
}