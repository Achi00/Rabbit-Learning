using RabbitMq.Contracts.Commands;
using RabbitMq.Contracts.Events;
using RabbitMq.Domain.Entity;
using RabbitMQ.Application.Enums;
using RabbitMqDemo.Persistance.Context;
using System.Text.Json;

namespace RabbitMQ.Application.Sagas
{
    public class OrderSagaCoordinator
    {
        private readonly MessageDbContext _context;

        public async Task OnStockReservedAsync(StockReservedEvent evt)
        {
            var saga = await _context.OrderSagaState.FindAsync(evt.SagaId);
            saga.CurrentStep = SagaStep.StockReserved;
            saga.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.OutboxMessages.AddAsync(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageType = "ChargePayment",
                // dummy data for testing
                Payload = JsonSerializer.Serialize(new ReserveStockCommand(evt.SagaId, evt.OrderId)),
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        public async Task OnPaymentFailedAsync(PaymentFailedEvent evt)
        {
            var saga = await _context.OrderSagaState.FindAsync(evt.SagaId);
            saga.CurrentStep = SagaStep.Compensating;
            saga.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.OutboxMessages.AddAsync(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageType = "ReleaseStock",
                Payload = JsonSerializer.Serialize(new ReleaseStockCommand(evt.SagaId, evt.OrderId)),
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        public async Task OnPaymentChargedAsync(PaymentChargedEvent evt)
        {
            var saga = await _context.OrderSagaState.FindAsync(evt.SagaId);
            saga.CurrentStep = SagaStep.Compensating;
            saga.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.OutboxMessages.AddAsync(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageType = "ChargePayment",
                Payload = JsonSerializer.Serialize(new ChargePaymentCommand(evt.SagaId, evt.OrderId, 1000, "Test@email.com")),
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        public async Task OnStockReservationFailedAsync(StockReservationFailedEvent evt)
        {
            var saga = await _context.OrderSagaState.FindAsync(evt.SagaId);
            // saga ends here, nothing to undo
            saga.CurrentStep = SagaStep.Cancelled;
            saga.UpdatedAt = DateTimeOffset.UtcNow;

            // no outbox, because nothing succeeded
        }


        public Task SaveAsync() => _context.SaveChangesAsync();
    }
}
