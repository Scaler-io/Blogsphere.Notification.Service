
using MassTransit;

namespace Blogsphere.Notification.Service.BackgroundJobs
{
    public class EventBusStarterJob(IBusControl busControl) : BackgroundService
    {
        private readonly IBusControl _busControl = busControl;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _busControl.StartAsync(stoppingToken);
            if(stoppingToken.IsCancellationRequested)
            {
                await _busControl.StopAsync(stoppingToken);
            }
            Console.WriteLine("BackgroundStarterJob running");
        }
    }
}