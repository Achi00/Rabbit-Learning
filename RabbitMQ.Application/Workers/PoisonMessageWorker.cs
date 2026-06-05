using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Application.Infrastructure;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitMQ.Application.Workers
{
    // bg worker
    public class PoisonMessageWorker : BackgroundService
    {
        private readonly RabbitMqConnectionProvider _provider;
        private readonly ILogger<PoisonMessageWorker> _logger;

        public PoisonMessageWorker(RabbitMqConnectionProvider provider, ILogger<PoisonMessageWorker> logger)
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
                var headers = ea.BasicProperties?.Headers;
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());

                _logger.LogError(
                    "Poison message | Body: {Body} | Retries: {Retries} | Reason: {Reason} | FailedAt: {FailedAt}",
                    body,
                    GetHeader(headers, "x-retry-count"),
                    GetHeader(headers, "x-failure-reason"),
                    GetHeader(headers, "x-failed-at")
                );
                // TODO: save in database in future
                await Task.CompletedTask;
            };

            await channel.BasicConsumeAsync("orders.poison", autoAck: false, consumer);
            await Task.Delay(Timeout.Infinite, ct);

            await channel.DisposeAsync();
        }

        private string GetHeader(IDictionary<string, object?>? headers, string key)
        {
            if (headers == null || !headers.TryGetValue(key, out var value))
            {
                return "unknown";
            }

            return value is byte[] bytes ? Encoding.UTF8.GetString(bytes) : value?.ToString() ?? "unknown";
        }
    }
}
