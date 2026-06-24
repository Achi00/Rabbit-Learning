using Microsoft.Extensions.Logging;
using RabbitMq.Contracts;
using RabbitMq.Contracts.Commands;
using RabbitMq.Contracts.Events;
using RabbitMq.Domain.Entity;
using RabbitMQ.Application.Sagas;
using RabbitMQ.Application.Services.Interfaces.Messages;
using RabbitMQ.Application.Services.Messages.Idempotency;
using RabbitMqDemo.Persistance.Context;
using System.Text.Json;

namespace RabbitMQ.Application.Handlers.StockHandlers
{
    public class ReleaseStockHandler : IMessageHandler
    {
        private readonly OrderSagaCoordinator _coordinator;
        private readonly DbIdempotencyService _idempotency;
        private readonly MessageDbContext _context;
        private readonly ILogger<ReleaseStockHandler> _logger;

        public ReleaseStockHandler(OrderSagaCoordinator coordinator, DbIdempotencyService idempotency, MessageDbContext context, ILogger<ReleaseStockHandler> logger)
        {
            _coordinator = coordinator;
            _idempotency = idempotency;
            _context = context;
            _logger = logger;
        }
        public async Task HandleAsync(JsonElement payload, Guid messageId)
        {
            // already seen this message id
            if (await _idempotency.IsDuplicateAsync(messageId))
            {
                return;
            }

            var command = payload.Deserialize<ReleaseStock>()
                ?? throw new InvalidOperationException($"Failed to deserialize {nameof(ReleaseStock)} from payload.");

            _logger.LogInformation("Handling {MessageType} for saga {SagaId}", nameof(ReleaseStockHandler), command.SagaId);


            await _context.OutboxMessages.AddAsync(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageType = MessageTypes.StockReleased,
                Payload = JsonSerializer.Serialize(new StockReleased(command.SagaId, command.OrderId)),
                CreatedAt = DateTimeOffset.UtcNow
            });

            _idempotency.MarkAsProcessed(messageId, MessageTypes.ReleaseStock);
            await _context.SaveChangesAsync();
        }
    }
}
