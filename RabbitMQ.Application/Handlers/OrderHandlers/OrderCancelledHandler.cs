using Microsoft.Extensions.Logging;
using RabbitMq.Domain.Entity;
using RabbitMQ.Application.Interfaces.Messages;
using RabbitMQ.Application.Services.Interfaces;
using RabbitMQ.Application.Services.Interfaces.Messages;
using RabbitMQ.Application.Services.Messages.Idempotency;
using RabbitMqDemo.Persistance.Context;
using System.Text.Json;

namespace RabbitMQ.Application.Handlers.OrderHandlers
{
    public class OrderCancelledHandler : IMessageHandler
    {
        private readonly IOrderCancelProcessor _orderCancelProcessor;
        // in memory idempotency check
        //private readonly InMemoryIdempotencyService _idempotency;
        private readonly DbIdempotencyService _idempotency;
        // db idempotency check
        private readonly MessageDbContext _context;
        private readonly ILogger<OrderCancelledHandler> _logger;

        public OrderCancelledHandler(
            IOrderCancelProcessor orderCancelProcessor,
            DbIdempotencyService idempotency,
            ILogger<OrderCancelledHandler> logger,
            MessageDbContext context
        )
        {
            _orderCancelProcessor = orderCancelProcessor;
            _idempotency = idempotency;
            _logger = logger;
            _context = context;
        }
        public async Task HandleAsync(JsonElement payload, Guid messageId)
        {
            if (await _idempotency.IsDuplicateAsync(messageId))
            {
                _logger.LogWarning("Duplicater message {MessageId}", messageId);
                return;
            }
            var order = payload.Deserialize<Order>() ?? throw new InvalidOperationException("Invalid OrderMessage payload");

            _logger.LogInformation("Cancelling order {OrderId} for {Email}", order.Id, order.ConsumerEmail);
            
            await _orderCancelProcessor.CancelOrderAsync(order);

            _idempotency.MarkAsProcessed(messageId, "OrderCancelled");

            await _context.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} Cancelled", order.Id);
        }
    }
}
