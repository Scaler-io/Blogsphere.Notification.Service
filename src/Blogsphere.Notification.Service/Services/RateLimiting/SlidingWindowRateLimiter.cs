using Blogsphere.Notification.Service.Configurations;
using Microsoft.Extensions.Options;

namespace Blogsphere.Notification.Service.Services.RateLimiting;

public sealed class SlidingWindowRateLimiter<T>(
    IOptions<RateLimiterOption> options,
    Func<RateLimiterOption, int> maxSelector) : IRateLimiter<T>
{
    private readonly RateLimiterOption _options = options.Value;
    private readonly Func<RateLimiterOption, int> _maxSelector = maxSelector;
    private readonly Queue<DateTimeOffset> _sentTimestamps = new();
    private readonly object _lock = new();

    public bool TryAcquire()
    {
        if (!_options.Enabled) return true;
        lock (_lock)
        {
            var windowStart = DateTimeOffset.UtcNow.AddMinutes(-1);
            while (_sentTimestamps.Count > 0 && _sentTimestamps.Peek() < windowStart)
                _sentTimestamps.Dequeue();

            if (_sentTimestamps.Count >= _maxSelector(_options))
                return false;

            _sentTimestamps.Enqueue(DateTimeOffset.UtcNow);
            return true;
        }
    }
}
