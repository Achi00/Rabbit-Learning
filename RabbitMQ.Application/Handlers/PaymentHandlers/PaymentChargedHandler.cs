using Microsoft.Extensions.Logging;
using RabbitMq.Contracts;
using RabbitMq.Contracts.Events;
using RabbitMQ.Application.Sagas;
using RabbitMQ.Application.Services.Interfaces.Messages;
using RabbitMQ.Application.Services.Messages.Idempotency;
using System.Text.Json;

namespace RabbitMQ.Application.Handlers.PaymentHandlers
{
    public class PaymentChargedHandler : IMessageHandler
    {
        private readonly OrderSagaCoordinator _coordinator;
        private readonly DbIdempotencyService _idempotency;
        private readonly ILogger<PaymentChargedHandler> _logger;


        public PaymentChargedHandler(OrderSagaCoordinator coordinator, DbIdempotencyService idempotency, ILogger<PaymentChargedHandler> logger)
        {
            _coordinator = coordinator;
            _idempotency = idempotency;
            _logger = logger;
        }

        public async Task HandleAsync(JsonElement payload, Guid messageId)
        {
            if (await _idempotency.IsDuplicateAsync(messageId))
            {
                return;
            }

            var evt = payload.Deserialize<PaymentChargedEvent>()
                ?? throw new InvalidOperationException($"Failed to deserialize {nameof(PaymentChargedEvent)}");

            // for testing
            _logger.LogInformation("Handling {MessageType} for saga {SagaId}", nameof(PaymentChargedHandler), evt.SagaId);


            await _coordinator.OnPaymentChargedAsync(evt);

            _idempotency.MarkAsProcessed(messageId, MessageTypes.PaymentCharged);

            await _coordinator.SaveAsync();
        }
    }
}
