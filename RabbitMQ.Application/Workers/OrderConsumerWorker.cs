using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Application.Infrastructure;
using RabbitMQ.Application.Models;
using RabbitMQ.Application.Services.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace RabbitMQ.Application.Workers
{
    // this is background service
    public class OrderConsumerWorker : BackgroundService
    {
        // our created provider
        private readonly RabbitMqConnectionProvider _provider;
        private readonly ILogger<OrderConsumerWorker> _logger;
        private readonly IOrderProcessor _orderProcessor;

        public OrderConsumerWorker(RabbitMqConnectionProvider provider, ILogger<OrderConsumerWorker> logger, IOrderProcessor orderProcessor)
        {
            _provider = provider;
            _logger = logger;
            _orderProcessor = orderProcessor;
        }
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            var channel = await _provider.Connection.CreateChannelAsync();
            var consumer = new AsyncEventingBasicConsumer(channel);

            // ack with condition
            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    // uses SerializeToUtf8Bytes avoids string allocation
                    // attempt to deserialize, if fails its permanent failure, no need to retry message because data is invalid
                    var order = MessageSerializer.Deserialize<OrderMessage>(ea.Body.ToArray());

                    // attempt order processing, will be transiet failure if this fails
                    // ProcessOrderAsync is set up to fail or succeed
                    await _orderProcessor.ProcessOrderAsync(order);

                    _logger.LogInformation("Processing: {order}", order);

                    // all good if we reach here, we can remove message from queue
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                // handle json formating errors, will not be retried
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Incorectly formed message, sending to poison queue");

                    var props = new BasicProperties
                    {
                        Persistent = true,
                        Headers = new Dictionary<string, object?>
                        {
                            { "x-failure-type", "permanent" },
                            { "x-failure-reason", ex.Message },
                            { "x-failed-at", DateTimeOffset.UtcNow.ToString("o") }
                        }
                    };

                    // add to poison
                    await channel.BasicPublishAsync("", "orders.poison", false, props, ea.Body.ToArray());
                    // remove from original prders.queue
                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                // handle gateway error, will be retried
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning(ex, "Payment gateway unavailable, will retry again");
                    await channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
                catch (Exception ex)
                {
                    // unexpected ex, will retry this later because it is unknown, so will be safer this way
                    // will be retried too
                    _logger.LogWarning(ex, "Transient failure, will retry");
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                }
            };

            await channel.BasicConsumeAsync("orders.queue", autoAck: false, consumer);
            // only stops if token passed
            await Task.Delay(Timeout.Infinite, ct);

            await channel.DisposeAsync();
        }
    }
}
