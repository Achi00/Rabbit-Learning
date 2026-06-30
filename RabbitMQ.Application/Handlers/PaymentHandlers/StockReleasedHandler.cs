using Microsoft.Extensions.Logging;
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
        private readonly ILogger<StockReleasedHandler> _logger;

        public StockReleasedHandler(OrderSagaCoordinator coordinator, DbIdempotencyService idempotency, ILogger<StockReleasedHandler> logger)
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

            var evt = payload.Deserialize<StockReleased>()
                ?? throw new InvalidOperationException($"Failed to deserialize {nameof(StockReleased)}");

            _logger.LogInformation("Handling {MessageType} for saga {CorrelationId}", nameof(StockReleased), evt.CorrelationId);

            await _coordinator.OnStockReleasedAsync(evt);

            _idempotency.MarkAsProcessed(messageId, MessageTypes.StockReleased);
        }
    }
}
