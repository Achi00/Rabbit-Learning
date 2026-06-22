using Microsoft.EntityFrameworkCore;
using RabbitMq.Contracts;
using RabbitMq.Contracts.Commands;
using RabbitMq.Contracts.Events;
using RabbitMq.Domain.Entity;
using RabbitMQ.Application.Enums;
using RabbitMQ.Application.Models;
using RabbitMqDemo.Persistance.Context;
using System.Text.Json;

namespace RabbitMQ.Application.Sagas
{
    public class OrderSagaCoordinator
    {
        private readonly MessageDbContext _context;

        public OrderSagaCoordinator(MessageDbContext context)
        {
            _context = context;
        }

        // saga starting point
        public async Task OnOrderCreatedAsync(OrderSubmittedEvent evt)
        {
            var sagaId = Guid.NewGuid();

            var sagaState = new OrderSagaState
            {
                SagaId = sagaId,
                CurrentStep = SagaStep.Started,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _context.OrderSagaState.AddAsync(sagaState);

            await _context.OutboxMessages.AddAsync(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageType = MessageTypes.ReserveStock,
                Payload = JsonSerializer.Serialize(new ReserveStockCommand(sagaId, evt.OrderId)),
                CreatedAt = DateTimeOffset.UtcNow
            });
        }


        // stock sagas
        // success
        // stock reservation succeeded, we call next step, charge payment command
        public async Task OnStockReservedAsync(StockReservedEvent evt)
        {
            var saga = await _context.OrderSagaState.FindAsync(evt.SagaId)
                ?? throw new InvalidOperationException($"Saga {evt.SagaId} not found");

            var order = await _context.Orders.FindAsync(evt.OrderId)
                ?? throw new InvalidOperationException($"Order {evt.OrderId} not found");

            saga.CurrentStep = SagaStep.StockReserved;
            saga.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.OutboxMessages.AddAsync(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageType = MessageTypes.ChargePayment,
                // pass next step
                Payload = JsonSerializer.Serialize(new ChargePaymentCommand(evt.SagaId, evt.OrderId, order.Amount, order.CustomerEmail)),
                CreatedAt = DateTimeOffset.UtcNow
            });
        }
        // failure
        public async Task OnStockReservationFailedAsync(StockReservationFailedEvent evt)
        {
            var saga = await _context.OrderSagaState.FindAsync(evt.SagaId)
                ?? throw new InvalidOperationException($"Saga {evt.SagaId} not found");

            // saga ends here, nothing to undo
            saga.CurrentStep = SagaStep.Cancelled;
            saga.UpdatedAt = DateTimeOffset.UtcNow;

            // no outbox, because nothing succeeded
        }

        // release stock
        public async Task OnStockReleasedAsync(StockReleasedEvent evt)
        {
            var saga = await _context.OrderSagaState.FindAsync(evt.SagaId)
                ?? throw new InvalidOperationException($"Saga {evt.SagaId} not found");

            // saga ends here, compensation confirmed
            saga.CurrentStep = SagaStep.Cancelled; 
            saga.UpdatedAt = DateTimeOffset.UtcNow;
            // no outbox
        }

        // payment sagas
        // success
        public async Task OnPaymentChargedAsync(PaymentChargedEvent evt)
        {
            var saga = await _context.OrderSagaState.FindAsync(evt.SagaId)
                ?? throw new InvalidOperationException($"Saga {evt.SagaId} not found");

            // final state
            saga.CurrentStep = SagaStep.Completed;
            saga.UpdatedAt = DateTimeOffset.UtcNow;

            // no outbox message, nothing left to do
        }
        // failure
        // payment failed, we call step before, releasing stock
        public async Task OnPaymentFailedAsync(PaymentFailedEvent evt)
        {
            var saga = await _context.OrderSagaState.FindAsync(evt.SagaId)
                ?? throw new InvalidOperationException($"Saga {evt.SagaId} not found");

            saga.CurrentStep = SagaStep.Compensating;
            saga.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.OutboxMessages.AddAsync(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageType = MessageTypes.ReleaseStock,
                Payload = JsonSerializer.Serialize(new ReleaseStockCommand(evt.SagaId, evt.OrderId)),
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        public Task SaveAsync() => _context.SaveChangesAsync();
    }
}
