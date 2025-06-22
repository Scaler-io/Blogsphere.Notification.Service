using MassTransit;

namespace Blogsphere.Notification.Service.BackgroundJobs
{
    public class EventBusStarterJob : IHostedService
    {
        private readonly IBusControl _busControl;

        public EventBusStarterJob(IBusControl busControl)
        {
            _busControl = busControl;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _busControl.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _busControl.StopAsync(cancellationToken);
        }
    }
}