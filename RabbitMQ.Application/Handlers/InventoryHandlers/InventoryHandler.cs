using Microsoft.Extensions.Logging;
using RabbitMq.Contracts;
using RabbitMq.Contracts.Commands;
using RabbitMq.Contracts.Events;
using RabbitMq.Domain.Entity;
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
        private readonly ILogger<InventoryHandler> _logger;


        public InventoryHandler(MessageDbContext context, DbIdempotencyService idempotency, ILogger<InventoryHandler> logger)
        {
            _context = context;
            _idempotency = idempotency;
            _logger = logger;
        }

        public async Task HandleAsync(JsonElement payload, Guid messageId)
        {
            if (await _idempotency.IsDuplicateAsync(messageId))
            {
                return;
            }

            // commands to controll certain operation, in this case reserving inventory stock if random value is successful
            var command = payload.Deserialize<ReserveStock>()
                ?? throw new InvalidOperationException($"Failed to deserialize {nameof(ReserveStock)} from payload.");

            _logger.LogInformation("Handling {MessageType} for saga {CorrelationId}", nameof(InventoryHandler), command.CorrelationId);


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
                    Payload = JsonSerializer.Serialize(new StockReserved(command!.CorrelationId, command.OrderId)),
                    CreatedAt = DateTimeOffset.UtcNow,
                };
            }
            else
            {
                outboxMessage = new OutboxMessage 
                {
                    Id = Guid.NewGuid(),
                    MessageType = MessageTypes.StockReservationFailed, 
                    Payload = JsonSerializer.Serialize(new StockReservationFailed(command!.CorrelationId, command.OrderId, "Out of stock")) ,
                    CreatedAt = DateTimeOffset.UtcNow,
                };
            }

            await _context.OutboxMessages.AddAsync(outboxMessage);

            _idempotency.MarkAsProcessed(messageId, "ReserveStock");

            await _context.SaveChangesAsync();
        }
    }
}
