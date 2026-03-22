using Blogsphere.Notification.Service.Configurations;
using Blogsphere.Notification.Service.Services;
using Microsoft.Extensions.Options;

namespace Blogsphere.Notification.Service.BackgroundJobs;

public class SmsProcessingJob(IOptions<AppConfigOption> appConfigOption, IServiceProvider serviceProvider) : BackgroundService
{
    private readonly AppConfigOption _appConfigOption = appConfigOption.Value;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();

            ISmsService smsService = scope.ServiceProvider.GetRequiredService<ISmsService>();
            await smsService.SendSmsAsync();

            await Task.Delay(_appConfigOption.IntervalUnit == "ss"
                ? TimeSpan.FromSeconds(_appConfigOption.NotificationProcessInterval)
                : TimeSpan.FromMinutes(_appConfigOption.NotificationProcessInterval), stoppingToken);
        }
    }
}
