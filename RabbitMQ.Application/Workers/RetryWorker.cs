using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Application.Infrastructure;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ.Application.Workers
{
    // bg worker
    public class RetryWorker : BackgroundService
    {
        private readonly RabbitMqConnectionProvider _provider;
        private readonly ILogger<RetryWorker> _logger;

        public RetryWorker(RabbitMqConnectionProvider provider, ILogger<RetryWorker> logger)
        {
            _provider = provider;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            var channel = await _provider.Connection.CreateChannelAsync();
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                var headers = ea.BasicProperties.Headers ?? new Dictionary<string, object>();
                var retryCount = headers.TryGetValue("x-retry-count", out var val) ? Convert.ToUInt32(val) : 0;

                var waitQueue = retryCount switch
                {
                    0 => "orders.retry.5s",
                    1 => "orders.retry.30s",
                    2 => "orders.retry.300s",
                    _ => null
                };

                if (waitQueue is null)
                {
                    _logger.LogWarning("Message pernamently failer after {retryCount} retries", retryCount);

                    var prop = new BasicProperties
                    {
                        Persistent = true,
                        Headers = new Dictionary<string, object?>
                        {
                            { "x-retry-count", retryCount },
                            { "x-failed-at", DateTimeOffset.UtcNow.ToString("O") },
                        }
                    };
                    await channel.BasicPublishAsync("", "orders.poison", false, prop, ea.Body.ToArray());
                    // after we store in orders.poison we drop message from main queue
                    await channel.BasicAckAsync(ea.DeliveryTag, false);

                    return;
                }

                _logger.LogInformation("Attempt {retryCount}, routing to {waitQueue}", retryCount, waitQueue);

                var retryProp = new BasicProperties
                {
                    Persistent = true,
                    Headers = new Dictionary<string, object?>
                    {
                        { "x-retry-count", retryCount + 1 }
                    }
                };

                // publish message to appropriate queue
                await channel.BasicPublishAsync("", waitQueue, false, retryProp, ea.Body.ToArray());
                // drop message from main queue
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            };

            await channel.BasicConsumeAsync("orders.dlq", autoAck: false, consumer);
            // only stops if token passed
            await Task.Delay(Timeout.Infinite, ct);

            await channel.DisposeAsync();
        }
    }
}
