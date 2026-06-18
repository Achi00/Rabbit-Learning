using RabbitMq.Contracts;
using RabbitMq.Contracts.Commands;
using RabbitMq.Contracts.Events;
using RabbitMq.Domain.Entity;
using RabbitMQ.Application.Enums;
using RabbitMQ.Application.Services.Interfaces.Messages;
using RabbitMQ.Application.Services.Messages.Idempotency;
using RabbitMqDemo.Persistance.Context;
using System.Text.Json;

namespace RabbitMQ.Application.Handlers.InventoryHandlers
{
    public class InventoryHandler : IMessageHandler
    {
        private readonly Random _random = new Random();
        private readonly MessageDbContext _context;
        private readonly DbIdempotencyService _idempotency;

        public InventoryHandler(MessageDbContext context, DbIdempotencyService idempotency)
        {
            _context = context;
            _idempotency = idempotency;
        }

        public async Task HandleAsync(JsonElement payload, Guid messageId)
        {
            if (await _idempotency.IsDuplicateAsync(messageId))
            {
                return;
            }

            // commands to controll certain operation, in this case reserving inventory stock if random value is successful
            var command = payload.Deserialize<ReserveStockCommand>();

            // simulate ~70% success rate
            var success = _random.Next(1, 10) > 3;

            OutboxMessage outboxMessage;

            // saga stock reserved if success = true
            if (success)
            {
                outboxMessage = new OutboxMessage 
                { 
                    Id = Guid.NewGuid(),
                    MessageType = MessageTypes.StockReserved, 
                    Payload = JsonSerializer.Serialize(new StockReservedEvent(command!.SagaId, command.OrderId)),
                    CreatedAt = DateTimeOffset.UtcNow,
                };
            }
            else
            {
                outboxMessage = new OutboxMessage 
                {
                    Id = Guid.NewGuid(),
                    MessageType = MessageTypes.StockReservationFailed, 
                    Payload = JsonSerializer.Serialize(new StockReservationFailedEvent(command!.SagaId, command.OrderId, "Out of stock")) ,
                    CreatedAt = DateTimeOffset.UtcNow,
                };
            }

            await _context.OutboxMessages.AddAsync(outboxMessage);

            _idempotency.MarkAsProcessed(messageId, "ReserveStock");

            await _context.SaveChangesAsync();
        }
    }
}
