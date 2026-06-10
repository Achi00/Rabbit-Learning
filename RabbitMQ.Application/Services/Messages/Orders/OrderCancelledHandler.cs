using Microsoft.Extensions.Logging;
using RabbitMq.Domain.Entity;
using RabbitMQ.Application.Interfaces.Messages;
using RabbitMQ.Application.Services.Interfaces;
using RabbitMQ.Application.Services.Interfaces.Messages;
using System.Text.Json;

namespace RabbitMQ.Application.Services.Messages.Orders
{
    public class OrderCancelledHandler : IMessageHandler
    {
        private readonly IOrderCancelProcessor _orderCancelProcessor;
        private readonly InMemoryIdempotencyService _idempotency;
        private readonly ILogger<OrderCancelledHandler> _logger;

        public OrderCancelledHandler(
            IOrderCancelProcessor orderCancelProcessor,
            InMemoryIdempotencyService idempotency,
            ILogger<OrderCancelledHandler> logger
        )
        {
            _orderCancelProcessor = orderCancelProcessor;
            _idempotency = idempotency;
            _logger = logger;
        }
        public async Task HandleAsync(JsonElement payload, Guid messageId)
        {
            if (_idempotency.IsDuplicate(messageId))
            {
                _logger.LogWarning("Duplicater message {MessageId}", messageId);
                return;
            }
            var order = payload.Deserialize<Order>() ?? throw new InvalidOperationException("Invalid OrderMessage payload");

            _logger.LogInformation("Cancelling order {OrderId} for {Email}", order.Id, order.CustomerEmail);
            await _orderCancelProcessor.CancelOrderAsync(order);

            _idempotency.MarkAsProcessed(messageId);
        }
    }
}
