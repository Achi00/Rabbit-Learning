using Microsoft.Extensions.Logging;
using RabbitMq.Domain.Entity;
using RabbitMQ.Application.Services.Interfaces;
using RabbitMQ.Application.Services.Interfaces.Messages;
using System.Text.Json;

namespace RabbitMQ.Application.Services.Messages.Orders
{
    public class OrderCreatedHandler : IMessageHandler
    {
        private readonly IOrderCreateProcessor _orderProcessor;
        private readonly InMemoryIdempotencyService _idempotency;
        private readonly ILogger<OrderCreatedHandler> _logger;

        public OrderCreatedHandler(
            IOrderCreateProcessor orderProcessor, 
            InMemoryIdempotencyService idempotency, 
            ILogger<OrderCreatedHandler> logger)
        {
            _orderProcessor = orderProcessor;
            _idempotency = idempotency;
            _logger = logger;
        }

        public async Task HandleAsync(JsonElement payload, Guid messageId)
        {
            
            // check is this message is seen before doing work
            if (_idempotency.IsDuplicate(messageId))
            {
                _logger.LogWarning("Duplicater message {MessageId}", messageId);
                // caller will ack this message, message should be handled at this poing
                return;
            }
            var order = payload.Deserialize<Order>() ?? throw new InvalidOperationException("Invalid OrderMessage payload");

            _logger.LogInformation("processing order {Order}", order);
            await _orderProcessor.ProcessOrderAsync(order);

            // record message as processed
            _idempotency.MarkAsProcessed(messageId);
        }
    }
}
