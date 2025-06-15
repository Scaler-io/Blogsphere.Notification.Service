using Blogsphere.Notification.Service.Configurations;

namespace Blogsphere.Notification.Service.DI;

public static class ServiceCollectionConfigurationExtensions
{
    public static IServiceCollection ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppConfigOption>(configuration.GetSection(AppConfigOption.OptionName))
        .Configure<ElasticSearchOption>(configuration.GetSection(ElasticSearchOption.OptionName))
        .Configure<EmailSettingOptions>(configuration.GetSection(EmailSettingOptions.OptionName))
        .Configure<EventBusOption>(configuration.GetSection(EventBusOption.OptionName))
        .Configure<EmailTemplates>(configuration.GetSection(EmailTemplates.OptionName))
        .Configure<BlobStorageOption>(configuration.GetSection(BlobStorageOption.OptionName));

        return services;
    }
}