using System.Text;
using Blogsphere.Notification.Service.Configurations;
using Blogsphere.Notification.Service.Extensions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Blogsphere.Notification.Service.BackgroundJobs;

public class ErrorQueueReprocessorJob(
    IOptions<ErrorQueueReprocessorOption> options,
    IOptions<EventBusOption> eventBusOptions,
    ILogger logger) : BackgroundService
{
    private const string ReprocessCountHeader = "x-reprocess-count";
    private const string ReprocessedAtHeader = "x-reprocessed-at";
    private const string OriginalExchangeHeader = "MT-OriginalExchange";
    private const string OriginalRoutingKeyHeader = "MT-OriginalRoutingKey";

    private readonly ErrorQueueReprocessorOption _options = options.Value;
    private readonly EventBusOption _eventBus = eventBusOptions.Value;
    private readonly ILogger _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.Here().Information("Error queue reprocessor is disabled");
            return;
        }

        if (_options.ErrorQueues == null || _options.ErrorQueues.Count == 0)
        {
            _logger.Here().Information("No error queues configured for reprocessing");
            return;
        }

        var interval = TimeSpan.FromSeconds(Math.Max(5, _options.PollIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessQueuesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.Here().Error(ex, "Error while reprocessing error queues");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    private async Task ProcessQueuesAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _eventBus.Host,
            VirtualHost = _eventBus.VirtualHost,
            UserName = _eventBus.Username,
            Password = _eventBus.Password
        };

        using var connection = await factory.CreateConnectionAsync(stoppingToken);
        using var channel = await connection.CreateChannelAsync(
            new CreateChannelOptions(publisherConfirmationsEnabled: false, publisherConfirmationTrackingEnabled: false, outstandingPublisherConfirmationsRateLimiter: null, consumerDispatchConcurrency: null),
            stoppingToken);

        foreach (var errorQueue in _options.ErrorQueues)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            await ProcessQueueAsync(channel, errorQueue, stoppingToken);
        }
    }

    private async Task ProcessQueueAsync(IChannel channel, string errorQueue, CancellationToken stoppingToken)
    {
        var processed = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            BasicGetResult result;
            try
            {
                result = await channel.BasicGetAsync(errorQueue, false, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.Here().Error(ex, "Unable to read from error queue {queueName}", errorQueue);
                return;
            }

            if (result == null)
            {
                break;
            }

            processed++;

            try
            {
                var headers = result.BasicProperties?.Headers;
                var reprocessCount = GetHeaderInt(headers, ReprocessCountHeader);

                if (reprocessCount >= _options.MaxAttempts)
                {
                    _logger.Here().Information("Max reprocess attempts reached for {queueName}, leaving in error queue", errorQueue);
                    await channel.BasicNackAsync(result.DeliveryTag, false, true, stoppingToken);
                    return;
                }

                var originalExchange = GetHeaderString(headers, OriginalExchangeHeader);
                var originalRoutingKey = GetHeaderString(headers, OriginalRoutingKeyHeader);
                var destinationExchange = string.IsNullOrWhiteSpace(originalExchange) ? string.Empty : originalExchange;
                var destinationRoutingKey = string.IsNullOrWhiteSpace(originalRoutingKey)
                    ? GetBaseQueueName(errorQueue)
                    : originalRoutingKey;

                var properties = BuildReprocessProperties(result.BasicProperties, reprocessCount + 1);
                await channel.BasicPublishAsync(destinationExchange, destinationRoutingKey, false, properties, result.Body, stoppingToken);
                await channel.BasicAckAsync(result.DeliveryTag, false, stoppingToken);

                _logger.Here().Information(
                    "Reprocessed message from {errorQueue} to {exchange}:{routingKey}",
                    errorQueue,
                    destinationExchange,
                    destinationRoutingKey);
            }
            catch (Exception ex)
            {
                _logger.Here().Error(ex, "Error while reprocessing message in {queueName}", errorQueue);
                await channel.BasicNackAsync(result.DeliveryTag, false, true, stoppingToken);
            }
        }

        if (processed > 0)
        {
            _logger.Here().Information("Processed {count} messages from {queueName}", processed, errorQueue);
        }
    }

    private static BasicProperties BuildReprocessProperties(IReadOnlyBasicProperties original, int reprocessCount)
    {
        var properties = new BasicProperties
        {
            DeliveryMode = original?.DeliveryMode ?? DeliveryModes.Persistent,
            ContentType = original?.ContentType,
            CorrelationId = original?.CorrelationId,
            MessageId = original?.MessageId,
            Type = original?.Type
        };

        var headers = original?.Headers == null
            ? []
            : new Dictionary<string, object>(original.Headers);

        headers[ReprocessCountHeader] = reprocessCount;
        headers[ReprocessedAtHeader] = Encoding.UTF8.GetBytes(DateTimeOffset.UtcNow.ToString("O"));

        properties.Headers = headers;

        return properties;
    }

    private static string GetHeaderString(IDictionary<string, object> headers, string key)
    {
        if (headers == null || !headers.TryGetValue(key, out var value) || value == null)
        {
            return null;
        }

        return value switch
        {
            byte[] bytes => Encoding.UTF8.GetString(bytes),
            ReadOnlyMemory<byte> memory => Encoding.UTF8.GetString(memory.ToArray()),
            _ => value.ToString()
        };
    }

    private static int GetHeaderInt(IDictionary<string, object> headers, string key)
    {
        if (headers == null || !headers.TryGetValue(key, out var value) || value == null)
        {
            return 0;
        }

        return value switch
        {
            int intValue => intValue,
            long longValue => (int)longValue,
            short shortValue => shortValue,
            byte byteValue => byteValue,
            sbyte sbyteValue => sbyteValue,
            byte[] bytes => int.TryParse(Encoding.UTF8.GetString(bytes), out var parsed) ? parsed : 0,
            ReadOnlyMemory<byte> memory => int.TryParse(Encoding.UTF8.GetString(memory.ToArray()), out var parsed) ? parsed : 0,
            string text => int.TryParse(text, out var parsed) ? parsed : 0,
            _ => int.TryParse(value.ToString(), out var parsed) ? parsed : 0
        };
    }

    private static string GetBaseQueueName(string errorQueue)
    {
        return errorQueue.EndsWith("_error", StringComparison.OrdinalIgnoreCase)
            ? errorQueue[..^6]
            : errorQueue;
    }
}
