namespace Blogsphere.Notification.Service.DI;

public static class IConfigurationExtensions
{
    public static IServiceCollection ConfigurationSettings(this IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
               .AddEnvironmentVariables()
               .Build();

        services.AddSingleton<IConfiguration>(configuration);

        return services;
    }
}
