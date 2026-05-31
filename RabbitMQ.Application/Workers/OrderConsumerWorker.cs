using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Application.Infrastructure;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitMQ.Application.Workers
{
    // this is background service
    public class OrderConsumerWorker : BackgroundService
    {
        // our created provider
        private readonly RabbitMqConnectionProvider _provider;
        private readonly ILogger<OrderConsumerWorker> _logger;

        public OrderConsumerWorker(RabbitMqConnectionProvider provider, ILogger<OrderConsumerWorker> logger)
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
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                _logger.LogInformation($"Processing: {message}");

                // simulate failure
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
            };

            await channel.BasicConsumeAsync("orders.queue", autoAck: false, consumer);
            // only stops if token passed
            await Task.Delay(Timeout.Infinite, ct);

            await channel.DisposeAsync();
        }
    }
}
