using Azure.Storage.Blobs;
using Blogsphere.Notification.Service.Data.Storage;
using Microsoft.Extensions.Azure;

namespace Blogsphere.Notification.Service.DI
{
    public static class ServiceCollectionDataExtensions
    {
        public static IServiceCollection ConfigureDataServices(this IServiceCollection services, IConfiguration configuration)
        {
            var blobConnectionString = configuration.GetConnectionString("BlobStorage");

            services.AddSingleton(_ => new BlobServiceClient(blobConnectionString));

            services.AddAzureClients(builder =>
            {
                builder.AddTableServiceClient(configuration.GetConnectionString("AzureTableStorage"))
                    .ConfigureOptions(options => options.Retry.MaxRetries = 3);
            });

            services.AddScoped(typeof(ITableRepository<>), typeof(TableRepository<>));
            services.AddScoped<IBlobRepository, BlobRepository>();

            return services;
        }
    }
}