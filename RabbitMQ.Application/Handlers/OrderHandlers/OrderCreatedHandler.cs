using Microsoft.Extensions.Logging;
using RabbitMq.Contracts;
using RabbitMq.Contracts.Events;
using RabbitMq.Domain.Entity;
using RabbitMQ.Application.Sagas;
using RabbitMQ.Application.Services.Interfaces;
using RabbitMQ.Application.Services.Interfaces.Messages;
using RabbitMQ.Application.Services.Messages.Idempotency;
using RabbitMqDemo.Persistance.Context;
using System.Text.Json;

namespace RabbitMQ.Application.Handlers.OrderHandlers
{
    public class OrderCreatedHandler : IMessageHandler
    {
        private readonly OrderSagaCoordinator _coordinator;
        private readonly DbIdempotencyService _idempotency;
        private readonly ILogger<OrderCreatedHandler> _logger;

        public OrderCreatedHandler(
            OrderSagaCoordinator coordinator,
            DbIdempotencyService idempotency, 
            ILogger<OrderCreatedHandler> logger)
        {
            _coordinator = coordinator;
            _idempotency = idempotency;
            _logger = logger;
        }

        public async Task HandleAsync(JsonElement payload, Guid messageId)
        {
            
            // check is this message is seen before doing work
            if (await _idempotency.IsDuplicateAsync(messageId))
            {
                _logger.LogWarning("Duplicater message {MessageId}", messageId);
                // caller will ack this message, message should be handled at this poing
                return;
            }

            var evt = payload.Deserialize<OrderCreatedEvent>() 
                ?? throw new InvalidOperationException($"Failed to deserialize {nameof(OrderCreatedEvent)}");

            await _coordinator.OnOrderCreatedAsync(evt);

            // record for idempotency
            _idempotency.MarkAsProcessed(messageId, MessageTypes.OrderCreated);

            _logger.LogInformation("Order {OrderId} Created", evt.OrderId);

            await _coordinator.SaveAsync();
        }
    }
}
