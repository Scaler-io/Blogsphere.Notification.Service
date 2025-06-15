using Blogsphere.Notification.Service.Configurations;
using Blogsphere.Notification.Service.EventBus.Consumers;
using Contracts.Events;
using MassTransit;

namespace Blogsphere.Notification.Service.DI
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

            services.AddSingleton<IConfiguration>(configuration);
            services.ConfigureOptions(configuration);

            services.AddMassTransit(config => 
            {
                config.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("notification", false));
                // load consumers from assembly
                config.AddConsumersFromNamespaceContaining<UserInvitationSentConsumer>();
                config.UsingRabbitMq((context, cfg) => 
                {
                    var rabbitMq = configuration.GetSection(EventBusOption.OptionName).Get<EventBusOption>();
                    cfg.Host(rabbitMq.Host, "/", host => 
                    {
                        host.Username(rabbitMq.Username);
                        host.Password(rabbitMq.Password);
                    });
                });
            });

            return services;
        }
    }
}