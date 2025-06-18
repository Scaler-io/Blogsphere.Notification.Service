using Blogsphere.Notification.Service.Configurations;
using Blogsphere.Notification.Service.EventBus.Consumers;
using Blogsphere.Notification.Service.Services;
using Contracts.Events;
using MassTransit;

namespace Blogsphere.Notification.Service.DI
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(config => 
            {
                config.SetKebabCaseEndpointNameFormatter();
                // load consumers from assembly
                config.AddConsumersFromNamespaceContaining<UserInvitationSentConsumer>();
                config.UsingRabbitMq((context, cfg) => 
                {
                    var rabbitMq = configuration.GetSection(EventBusOption.OptionName).Get<EventBusOption>();
                    cfg.Host(rabbitMq.Host, rabbitMq.VirtualHost, host => 
                    {
                        host.Username(rabbitMq.Username);
                        host.Password(rabbitMq.Password);
                    });

                    cfg.UseMessageRetry(x => x.Interval(3, 3000));
                    cfg.ConfigureEndpoints(context);
                });
            });

            services.AddScoped<IEmailService, EmailService>();

            return services;
        }
    }
}