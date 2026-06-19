using Microsoft.Extensions.Logging;
using RabbitMq.Contracts.Events;
using RabbitMQ.Application.Sagas;
using RabbitMQ.Application.Services.Interfaces.Messages;
using RabbitMQ.Application.Services.Messages.Idempotency;
using System.Text.Json;

namespace RabbitMQ.Application.Services.Messages.OrderHandlers
{
    public class StockReservationFailedHandler : IMessageHandler
    {
        private readonly OrderSagaCoordinator _coordinator;
        private readonly DbIdempotencyService _idempotency;
        private readonly ILogger<StockReservationFailedHandler> _logger;

        public StockReservationFailedHandler(OrderSagaCoordinator coordinator, DbIdempotencyService idempotency, ILogger<StockReservationFailedHandler> logger)
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

            var evt = payload.Deserialize<StockReservationFailedEvent>()
                ?? throw new InvalidOperationException($"Failed to deserialize {nameof(StockReservationFailedEvent)}");

            _logger.LogInformation("Handling {MessageType} for saga {SagaId}", nameof(StockReservationFailedHandler), evt.SagaId);


            await _coordinator.OnStockReservationFailedAsync(evt);

            _idempotency.MarkAsProcessed(messageId, "StockReservationFailed");
            await _coordinator.SaveAsync();
        }
    }
}
