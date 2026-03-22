namespace Blogsphere.Notification.Service.Services.RateLimiting;

public interface IRateLimiter<T>
{
    bool TryAcquire();
}
