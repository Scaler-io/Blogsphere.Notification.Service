using Blogsphere.Notification.Service.Data.Storage;
using Microsoft.Extensions.Azure;

namespace Blogsphere.Notification.Service.DI
{
    public static class ServiceCollectionDataExtensions
    {
        public static IServiceCollection ConfigureDataServices(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddAzureClients(builder =>
            {
                builder.AddBlobServiceClient(configuration.GetConnectionString("BlobStorage"))
                    .ConfigureOptions(options => options.Retry.MaxRetries = 3);

                builder.AddTableServiceClient(configuration.GetConnectionString("AzureTableStorage"))
                    .ConfigureOptions(options => options.Retry.MaxRetries = 3);
            });

            services.AddScoped(typeof(ITableRepository<>), typeof(TableRepository<>));
            services.AddScoped<IBlobRepository, BlobRepository>();

            return services;
        }
    }
}