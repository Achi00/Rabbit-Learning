using RabbitMq.Contracts;
using RabbitMq.Contracts.Events;
using RabbitMQ.Application.Sagas;
using RabbitMQ.Application.Services.Interfaces.Messages;
using RabbitMQ.Application.Services.Messages.Idempotency;
using System.Text.Json;

namespace RabbitMQ.Application.Handlers.PaymentHandlers
{
    public class StockReleasedHandler : IMessageHandler
    {
        private readonly OrderSagaCoordinator _coordinator;
        private readonly DbIdempotencyService _idempotency;

        public StockReleasedHandler(OrderSagaCoordinator coordinator, DbIdempotencyService idempotency)
        {
            _coordinator = coordinator;
            _idempotency = idempotency;
        }
        public async Task HandleAsync(JsonElement payload, Guid messageId)
        {
            if (await _idempotency.IsDuplicateAsync(messageId))
            {
                return;
            }

            var evt = payload.Deserialize<StockReleasedEvent>()
                ?? throw new InvalidOperationException($"Failed to deserialize {nameof(StockReleasedEvent)}");

            await _coordinator.OnStockReleasedAsync(evt);

            _idempotency.MarkAsProcessed(messageId, MessageTypes.StockReleased);
        }
    }
}
