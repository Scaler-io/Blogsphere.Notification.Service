using Azure.Storage.Blobs;

namespace Blogsphere.Notification.Service.Data.Storage;

public class AzureBlobStorageService(BlobServiceClient blobServiceClient, IBlobStorageService innerService) : IBlobStorageService
{
    private const string ContainerName = "blogsphere";
    private readonly BlobServiceClient _blobServiceClient = blobServiceClient;
    private readonly IBlobStorageService _innerService = innerService;

    public async Task<Stream> GetBlobAsync(string path)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
            var blobClient = containerClient.GetBlobClient(path);
            var response = await blobClient.DownloadStreamingAsync();
            var memoryStream = new MemoryStream();
            await response.Value.Content.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }
        catch
        {
            return await _innerService.GetBlobAsync(path);
        }
    }
}
