using Microsoft.Extensions.Logging;
using RabbitMq.Domain.Entity;
using RabbitMQ.Application.Services.Interfaces;
using RabbitMQ.Application.Services.Interfaces.Messages;
using RabbitMQ.Application.Services.Messages.Idempotency;
using RabbitMqDemo.Persistance.Context;
using System.Text.Json;

namespace RabbitMQ.Application.Services.Messages.Orders
{
    public class OrderCreatedHandler : IMessageHandler
    {
        private readonly MessageDbContext _context;
        private readonly IOrderCreateProcessor _orderProcessor;
        private readonly DbIdempotencyService _idempotency;
        private readonly ILogger<OrderCreatedHandler> _logger;

        public OrderCreatedHandler(
            MessageDbContext context,
            IOrderCreateProcessor orderProcessor,
            DbIdempotencyService idempotency, 
            ILogger<OrderCreatedHandler> logger)
        {
            _context = context;
            _orderProcessor = orderProcessor;
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
            var order = payload.Deserialize<Order>() ?? throw new InvalidOperationException("Invalid OrderMessage payload");

            // begin transaction
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.Orders.AddAsync(new Order 
                { 
                    Id = order.Id,
                    Amount = order.Amount,
                    CustomerEmail = order.CustomerEmail
                });

                // record for idempotency
                _idempotency.MarkAsProcessed(messageId, "OrderCreated");

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Order {OrderId}", order.Id);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
