using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Application.Infrastructure;
using RabbitMQ.Application.Infrastructure.Envelope;
using RabbitMQ.Application.Models;
using RabbitMQ.Application.Services.Interfaces;
using RabbitMQ.Application.Services.Interfaces.Messages;
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
        private readonly IOrderCreateProcessor _orderProcessor;
        private readonly IServiceProvider _serviceProvider;

        public OrderConsumerWorker(
            RabbitMqConnectionProvider provider, 
            ILogger<OrderConsumerWorker> logger, 
            IOrderCreateProcessor orderProcessor,
            IServiceProvider serviceProvider
            )
        {
            _provider = provider;
            _logger = logger;
            _orderProcessor = orderProcessor;
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            var channel = await _provider.Connection.CreateChannelAsync();
            var consumer = new AsyncEventingBasicConsumer(channel);

            // create scope per message
            await using var scope = _serviceProvider.CreateAsyncScope();

            // ack with condition
            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var envelope = MessageSerializer.Deserialize<MessageEnvelope>(ea.Body.ToArray());
                    /*
                     * even this bg service is singleton, creating scoped service directly 
                     * from singleton each message should have 
                     * its own short lived DI container 
                     */
                    // will grab type based on what value it has passed in envelope.MessageType e.g "OrderCreated"
                    var handler = scope.ServiceProvider.GetKeyedService<IMessageHandler>(envelope.MessageType)
                        ?? throw new InvalidOperationException($"No handler registered for {envelope.MessageType}");

                    // passing message id same as envelope, not bussiness id, this will avoid collisions between messages
                    await handler.HandleAsync(envelope.Payload, envelope.MessageId);

                    _logger.LogInformation("Choosing handler for type: {Handler}", envelope.MessageType);

                    // all good if we reach here, we can remove message from queue
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                // handle json formating errors, will not be retried
                // goes to poison
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
