using Microsoft.Extensions.Logging;
using RabbitMq.Contracts;
using RabbitMq.Contracts.Events;
using RabbitMQ.Application.Sagas;
using RabbitMQ.Application.Services.Interfaces.Messages;
using RabbitMQ.Application.Services.Messages.Idempotency;
using System.Text.Json;

namespace RabbitMQ.Application.Handlers.PaymentHandlers
{
    public class PaymentFailedHandler : IMessageHandler
    {
        private readonly OrderSagaCoordinator _coordinator;
        private readonly DbIdempotencyService _idempotency;
        private readonly ILogger<PaymentFailedHandler> _logger;

        public PaymentFailedHandler(OrderSagaCoordinator coordinator, DbIdempotencyService idempotency, ILogger<PaymentFailedHandler> logger)
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

            var evt = payload.Deserialize<PaymentFailed>()
                ?? throw new InvalidOperationException($"Failed to deserialize {nameof(PaymentFailed)}");

            _logger.LogInformation("Handling {MessageType} for saga {CorrelationId}", nameof(PaymentFailedHandler), evt.CorrelationId);


            await _coordinator.OnPaymentFailedAsync(evt);

            _idempotency.MarkAsProcessed(messageId, MessageTypes.PaymentFailed);

            await _coordinator.SaveAsync();
        }
    }
}
