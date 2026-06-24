using Microsoft.Extensions.Logging;
using RabbitMq.Contracts;
using RabbitMq.Contracts.Events;
using RabbitMQ.Application.Sagas;
using RabbitMQ.Application.Services.Interfaces.Messages;
using RabbitMQ.Application.Services.Messages.Idempotency;
using System.Text.Json;

namespace RabbitMQ.Application.Handlers.StockHandlers
{
    public class StockReservedHandler : IMessageHandler
    {
        private readonly OrderSagaCoordinator _coordinator;
        private readonly DbIdempotencyService _idempotency;
        private readonly ILogger<StockReservedHandler> _logger;

        public StockReservedHandler(OrderSagaCoordinator coordinator, DbIdempotencyService idempotency, ILogger<StockReservedHandler> logger)
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

            var evt = payload.Deserialize<StockReserved>() 
                ?? throw new InvalidOperationException($"Failed to deserialize {nameof(StockReserved)}");

            _logger.LogInformation("Handling {MessageType} for saga {SagaId}", nameof(StockReservedHandler), evt.SagaId);


            await _coordinator.OnStockReservedAsync(evt);

            _idempotency.MarkAsProcessed(messageId, MessageTypes.StockReserved);
            await _coordinator.SaveAsync();
        }
    }
}
