using Azure.Storage.Blobs;

namespace Blogsphere.Notification.Service.Data.Storage;

public class BlobRepository(BlobServiceClient blobServiceClient) : IBlobRepository
{
    private readonly BlobServiceClient _blobServiceClient = blobServiceClient;

    private async Task<BlobContainerClient> GetContainerClientAsync(string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();
        return containerClient;
    }

    public async Task<Stream> GetBlobAsync(string containerName, string blobName)
    {
        var containerClient = await GetContainerClientAsync(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        var response = await blobClient.DownloadStreamingAsync();
        var memoryStream = new MemoryStream();
        await response.Value.Content.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task UploadBlobAsync(string containerName, string blobName, Stream stream)
    {
        var containerClient = await GetContainerClientAsync(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(stream, overwrite: true);
    }

    public async Task DeleteBlobAsync(string containerName, string blobName)
    {
        var containerClient = await GetContainerClientAsync(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }
}
