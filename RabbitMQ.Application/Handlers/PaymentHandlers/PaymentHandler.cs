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

        public PaymentHandler(MessageDbContext context, DbIdempotencyService idempotency)
        {
            _context = context;
            _idempotency = idempotency;
        }

        public async Task HandleAsync(JsonElement payload, Guid messageId)
        {
            // idempotency check, see if message id was already seen
            if (await _idempotency.IsDuplicateAsync(messageId))
            {
                return;
            }

            var command = payload.Deserialize<ChargePaymentCommand>()
                ?? throw new InvalidOperationException($"Failed to deserialize {nameof(ChargePaymentCommand)} from payload.");

            // randomize success by ~70%
            var success = _random.Next(1, 10) > 3;

            var outboxMessage = success
                ? new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    MessageType = MessageTypes.PaymentCharged,
                    Payload = JsonSerializer.Serialize(new PaymentChargedEvent(command.SagaId, command.OrderId))
                }
                : new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    MessageType = MessageTypes.PaymentFailed,
                    Payload = JsonSerializer.Serialize(new PaymentFailedEvent(command.SagaId, command.OrderId, "Card Declined"))
                };

            await _context.OutboxMessages.AddAsync(outboxMessage);
            _idempotency.MarkAsProcessed(messageId, MessageTypes.ChargePayment);
            await _context.SaveChangesAsync();
        }
    }
}
