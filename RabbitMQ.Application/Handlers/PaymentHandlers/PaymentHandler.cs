using Microsoft.Extensions.Logging;
using RabbitMq.Contracts;
using RabbitMq.Contracts.Commands;
using RabbitMq.Contracts.Events;
using RabbitMq.Domain.Entity;
using RabbitMQ.Application.Services.Interfaces.Messages;
using RabbitMQ.Application.Services.Messages.Idempotency;
using RabbitMqDemo.Persistance.Context;
using System.Text.Json;

namespace RabbitMQ.Application.Handlers.PaymentHandlers
{
    public class PaymentHandler : IMessageHandler
    {
        private readonly Random _random = new Random();
        private readonly MessageDbContext _context;
        private readonly DbIdempotencyService _idempotency;
        private readonly ILogger<PaymentHandler> _logger;

        public PaymentHandler(MessageDbContext context, DbIdempotencyService idempotency, ILogger<PaymentHandler> logger)
        {
            _context = context;
            _idempotency = idempotency;
            _logger = logger;
        }

        public async Task HandleAsync(JsonElement payload, Guid messageId)
        {

            // idempotency check, see if message id was already seen
            if (await _idempotency.IsDuplicateAsync(messageId))
            {
                return;
            }

            var command = payload.Deserialize<ChargePayment>()
                ?? throw new InvalidOperationException($"Failed to deserialize {nameof(ChargePayment)} from payload.");

            _logger.LogInformation("Handling {MessageType} for saga {CorrelationId}", nameof(PaymentHandler), command.CorrelationId);

            // randomize success by ~70%
            var success = _random.Next(1, 10) > 3;

            var outboxMessage = success
                ? new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    MessageType = MessageTypes.PaymentCharged,
                    Payload = JsonSerializer.Serialize(new PaymentCharged(command.CorrelationId, command.OrderId))
                }
                : new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    MessageType = MessageTypes.PaymentFailed,
                    Payload = JsonSerializer.Serialize(new PaymentFailed(command.CorrelationId, command.OrderId, "Card Declined"))
                };

            await _context.OutboxMessages.AddAsync(outboxMessage);
            _idempotency.MarkAsProcessed(messageId, MessageTypes.ChargePayment);
            await _context.SaveChangesAsync();
        }
    }
}
