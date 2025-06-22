namespace Blogsphere.Notification.Service.Data.Storage;

public interface IBlobStorageService
{
    Task<Stream> GetBlobAsync(string path);
}