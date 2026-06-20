using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMq.Contracts;
using RabbitMq.Contracts.Events;
using RabbitMq.Domain.Entity;
using RabbitMQ.Application.Enums;
using RabbitMQ.Application.Infrastructure;
using RabbitMQ.Application.Infrastructure.Envelope;
using RabbitMQ.Client;
using RabbitMqDemo.Persistance.Context;
using System.Text.Json;

namespace RabbitMQ.Application.Workers
{
    // this is part of producer side retry and will be its own seperated from consumer base queue retries
    // order producer with separated retry logic
    public class OrderProducerWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OrderProducerWorker> _logger;

        public OrderProducerWorker(IServiceScopeFactory scopeFactory, ILogger<OrderProducerWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        // move channle set up in RabbitMqPublisher service
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            var sent = false;

            while (!ct.IsCancellationRequested)
            {
                if (!sent)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<MessageDbContext>();

                    var order = new Order
                    {
                        Id = Guid.NewGuid(),
                        Amount = 100,
                        CustomerEmail = "mail@gmail.com"
                    };

                    // fixes naming missmatch from Order type field Id and OrderId in events
                    var orderCreatedEvent = new OrderCreatedEvent(order.Id);

                    var outboxMessage = new OutboxMessage
                    {
                        Id = Guid.NewGuid(),
                        MessageType = MessageTypes.OrderCreated,
                        Payload = JsonSerializer.Serialize(orderCreatedEvent),
                        CreatedAt = DateTimeOffset.UtcNow,
                        SentAt = null
                    };

                    await db.Orders.AddAsync(order, ct);
                    await db.OutboxMessages.AddAsync(outboxMessage, ct);
                    // atomic, both or none!!!
                    await db.SaveChangesAsync(ct);

                    _logger.LogInformation("Order {OrderId} written with outbox message {MessageId}", order.Id, outboxMessage.Id);

                    sent = true;
                }
                await Task.Delay(1000, ct);
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
