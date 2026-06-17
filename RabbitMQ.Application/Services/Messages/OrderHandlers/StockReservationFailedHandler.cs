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

        public async Task HandleAsync(JsonElement payload, Guid messageId)
        {
            if (await _idempotency.IsDuplicateAsync(messageId)) return;

            var evt = payload.Deserialize<StockReservationFailedEvent>()!;
            await _coordinator.OnStockReservedAsync(evt);

            _idempotency.MarkAsProcessed(messageId, "StockReservationFailed");
            await _coordinator.SaveAsync();
        }
    }
}
