using Blogsphere.Notification.Service.Configurations;
using Blogsphere.Notification.Service.EventBus.Consumers;
using Blogsphere.Notification.Service.Services;
using Contracts.Events;
using MassTransit;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

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

            // open telemtry
            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService("blogsphere.notification.service"))
                .WithTracing(tracing =>
                {
                    tracing.AddSource("Blogsphere.Notification.Service")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation(options => options.SetDbStatementForText = true)
                    .AddZipkinExporter(options =>
                    {
                        options.Endpoint = new Uri(configuration["Zipkin:Url"]);
                    });
                });

            return services;
        }
    }
}