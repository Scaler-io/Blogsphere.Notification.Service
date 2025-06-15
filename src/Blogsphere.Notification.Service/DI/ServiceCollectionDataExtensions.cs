using Amazon.S3;
using Blogsphere.Notification.Service.Configurations;
using Blogsphere.Notification.Service.Data;
using Blogsphere.Notification.Service.Data.Storage;
using Microsoft.EntityFrameworkCore;

namespace Blogsphere.Notification.Service.DI
{
    public static class ServiceCollectionDataExtensions
    {
        public static IServiceCollection ConfigureDataSevices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<NotificationDbContext>(options => 
            {
                options.UseSqlServer(configuration.GetConnectionString("SqlServer"), options => {
                    options.MigrationsHistoryTable("__EFMigrationsHistory", "blogsphere");
                });
            });

            services.AddTransient(sp => 
            {
                var blobStorageOption = configuration.GetSection(BlobStorageOption.OptionName).Get<BlobStorageOption>();
                var config = new AmazonS3Config
                {
                    ServiceURL = blobStorageOption.ServiceUrl, // MinIO endpoint
                    ForcePathStyle = true,                // Required for MinIO
                    SignatureMethod = Amazon.Runtime.SigningAlgorithm.HmacSHA256
                };
                return new AmazonS3Client(blobStorageOption.UserName, blobStorageOption.Password, config);
            });

            services.AddScoped<IBlobStorageService, BlobStorageService>();

            return services;
        }
    }
}