using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Application.Enums;
using RabbitMQ.Application.Infrastructure;
using RabbitMQ.Application.Infrastructure.Envelope;
using RabbitMQ.Application.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace RabbitMQ.Application.Workers
{
    // this is part of producer side retry and will be its own seperated from consumer base queue retries
    // order producer with separated retry logic
    public class OrderProducerWorker : BackgroundService
    {
        private readonly RabbitMqConnectionProvider _connectionProvider;
        private readonly ILogger<OrderProducerWorker> _logger;

        public OrderProducerWorker(RabbitMqConnectionProvider connectionProvider, ILogger<OrderProducerWorker> logger)
        {
            _connectionProvider = connectionProvider;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            // channel options, activate publisher confirms
            var channelOptions = new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true
            );

            await using var channel = await _connectionProvider.Connection.CreateChannelAsync(channelOptions);
            bool sent = false;
            while (!ct.IsCancellationRequested)
            {
                // test single message send for idempotency
                if (!sent)
                {
                    var order = new OrderMessage
                    {
                        Id = Guid.NewGuid(),
                        Amount = 100,
                        CustomerEmail = "mail@gmail.com"
                    };

                    // switch between create and cancel order
                    var messageType = DateTime.UtcNow.Second % 2 == 0
                        ? MessageTypes.OrderCreated
                        : MessageTypes.OrderCancelled;

                    var envelope = new MessageEnvelope
                    {
                        // determines type and which handler/service to use hor this message
                        MessageType = messageType.ToString(),
                        Payload = JsonSerializer.SerializeToElement(order)
                    };
                    //var body = MessageSerializer.Serialize(order);
                    await PublishWithRetryAsync(channel, envelope, ct);
                    sent = true;
                }
                await Task.Delay(2000, ct);
            }
        }

        // helper
        private async Task PublishWithRetryAsync(IChannel channel, MessageEnvelope envelope, CancellationToken ct)
        {
            var body = MessageSerializer.Serialize(envelope);
            var attempts = 0;
            const int maxAttempts = 3;

            while (attempts < maxAttempts)
            {
                try
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    cts.CancelAfter(TimeSpan.FromSeconds(5));

                    // add props for future pland like this envelope pattern and also for idempotency in future
                    var props = new BasicProperties
                    {
                        Persistent = true,
                        ContentType = "application/json",
                        MessageId = envelope.MessageId.ToString()
                    };


                    // after introduced channelOptions in this specs this will wait for broker confirmation before returning
                    await channel.BasicPublishAsync(
                        exchange: "orders.exchange", 
                        routingKey: "orders", 
                        mandatory: false,
                        basicProperties: props,
                        body: body, 
                        cancellationToken: cts.Token
                    );

                    _logger.LogInformation("Confirmed: {Type} {Id}", envelope.MessageType, envelope.MessageId);
                    return;
                }
                // only if operation cancelation is caused by token
                catch (OperationCanceledException) when (!ct.IsCancellationRequested)
                {
                    attempts++;
                    _logger.LogWarning("Connection timeout on attempt {Attempt} for {Body}", attempts, body);
                    await Task.Delay(attempts * 2000, ct);
                }
            }
            _logger.LogError("Failed to confirm {Type} {Id} after {Max} attempts", envelope.MessageType, envelope.MessageId, maxAttempts);
        }
    }
}
