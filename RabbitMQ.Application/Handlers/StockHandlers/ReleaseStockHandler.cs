using RabbitMq.Contracts.Commands;
using RabbitMq.Contracts.Events;
using RabbitMQ.Application.Sagas;
using RabbitMQ.Application.Services.Interfaces.Messages;
using RabbitMQ.Application.Services.Messages.Idempotency;
using RabbitMqDemo.Persistance.Context;
using RabbitMq.Domain.Entity;
using System.Text.Json;
using RabbitMq.Contracts;

namespace RabbitMQ.Application.Handlers.StockHandlers
{
    public class ReleaseStockHandler : IMessageHandler
    {
        private readonly OrderSagaCoordinator _coordinator;
        private readonly DbIdempotencyService _idempotency;
        private readonly MessageDbContext _context;

        public ReleaseStockHandler(OrderSagaCoordinator coordinator, DbIdempotencyService idempotency, MessageDbContext context)
        {
            _coordinator = coordinator;
            _idempotency = idempotency;
            _context = context;
        }
        public async Task HandleAsync(JsonElement payload, Guid messageId)
        {
            // already seen this message id
            if (await _idempotency.IsDuplicateAsync(messageId))
            {
                return;
            }

            var command = payload.Deserialize<ReleaseStockCommand>()
                ?? throw new InvalidOperationException($"Failed to deserialize {nameof(ReleaseStockCommand)} from payload.");


            await _context.OutboxMessages.AddAsync(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageType = MessageTypes.StockReleased,
                Payload = JsonSerializer.Serialize(new StockReleasedEvent(command.SagaId, command.OrderId)),
                CreatedAt = DateTimeOffset.UtcNow
            });

            _idempotency.MarkAsProcessed(messageId, MessageTypes.ReleaseStock);
            await _context.SaveChangesAsync();
        }
    }
}
