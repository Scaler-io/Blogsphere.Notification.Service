
using Amazon.S3;
using Amazon.S3.Model;
using Blogsphere.Notification.Service.Configurations;
using Microsoft.Extensions.Options;

namespace Blogsphere.Notification.Service.Data.Storage;

public class BlobStorageService(AmazonS3Client s3Client, IOptions<BlobStorageOption> blobStorageOption) : IBlobStorageService
{
    private readonly AmazonS3Client _s3Client = s3Client;
    private readonly BlobStorageOption _blobStorageOption = blobStorageOption.Value;

    public async Task<Stream> GetBlobAsync(string path)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _blobStorageOption.BucketName,
                Key = path
            };
            using var response = await _s3Client.GetObjectAsync(request);
            var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}