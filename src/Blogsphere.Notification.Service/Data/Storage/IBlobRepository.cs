namespace Blogsphere.Notification.Service.Data.Storage;

public interface IBlobRepository
{
    Task<Stream> GetBlobAsync(string containerName, string blobName);
    Task UploadBlobAsync(string containerName, string blobName, Stream stream);
    Task DeleteBlobAsync(string containerName, string blobName);
}
