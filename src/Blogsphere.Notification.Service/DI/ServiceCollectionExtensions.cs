using Blogsphere.Notification.Service.Configurations;
using Blogsphere.Notification.Service.EventBus.Consumers;
using Blogsphere.Notification.Service.Services;
using Blogsphere.Notification.Service.Services.RateLimiting;
using Blogsphere.Notification.Service.Services.Sanitization;
using Blogsphere.Notification.Service.Services.Validation;
using MassTransit;
using Microsoft.Extensions.Options;
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
            services.AddScoped<ISmsService, SmsService>();
            services.AddSingleton<ISmtpClientFactory, SmtpClientFactory>();
            services.AddSingleton<IRateLimiter<EmailRateLimit>>(sp =>
                new SlidingWindowRateLimiter<EmailRateLimit>(
                    sp.GetRequiredService<IOptions<RateLimiterOption>>(),
                    options => options.MaxEmailsPerMinute));
            services.AddSingleton<IRateLimiter<SmsRateLimit>>(sp =>
                new SlidingWindowRateLimiter<SmsRateLimit>(
                    sp.GetRequiredService<IOptions<RateLimiterOption>>(),
                    options => options.MaxSmsPerMinute));
            services.AddSingleton<IValidationService, ValidationService>();
            services.AddSingleton<IHtmlSanitizer, HtmlSanitizer>();

            // open telemtry
            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService("blogsphere.notification.service"))
                .WithTracing(tracing =>
                {
                    tracing.AddSource("Blogsphere.Notification.Service")
                    .AddSource("Azure.*")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddJaegerExporter(options =>
                    {
                        options.AgentHost = configuration["Jaeger:Host"];
                        options.AgentPort = int.Parse(configuration["Jaeger:Port"]);
                    });
                });

            return services;
        }
    }
}