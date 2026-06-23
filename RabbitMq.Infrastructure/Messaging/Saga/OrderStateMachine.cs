using MassTransit;
using RabbitMq.Contracts.Commands;
using RabbitMq.Contracts.Events;

namespace RabbitMq.Infrastructure.Messaging.Saga
{
    public class OrderStateMachine : MassTransitStateMachine<OrderSagaState>
    {
        // this states replace old enum SagaStep
        public State StockReserving { get; private set; }
        public State PaymentCharging { get; private set; }
        public State Completed { get; private set; }
        public State Cancelled { get; private set; }
        public State Compensating { get; private set; }
        public State Compensated { get; private set; }

        // events, each maps to message type
        public Event<OrderSubmittedEvent> OrderSubmitted { get; private set; }
        public Event<OrderCompletedEvent> OrderCompleted { get; private set; }
        public Event<OrderCancelledEvent> OrderCancelled { get; private set; }
        public Event<StockReservedEvent> StockReserved { get; private set; }
        public Event<StockReservationFailedEvent> StockReservationFailed { get; private set; }
        public Event<PaymentChargedEvent> PaymentCharged { get; private set; }
        public Event<PaymentFailedEvent> PaymentFailed { get; private set; }
        public Event<StockReleasedEvent> StockReleased { get; private set; }

        protected OrderStateMachine()
        {
            // current state string
            InstanceState(x => x.CurrentState);

            // correlate incoming messages to correct saga instance
            Event(() => OrderSubmitted, x =>
            {
                x.CorrelateById(context => context.Message.OrderId);

                x.SelectId(context => context.Message.OrderId);
            });

            Event(() => StockReserved, x => x.CorrelateById(ctx => ctx.Message.SagaId));
            Event(() => StockReservationFailed, x => x.CorrelateById(ctx => ctx.Message.SagaId));
            Event(() => PaymentCharged, x => x.CorrelateById(ctx => ctx.Message.SagaId));
            Event(() => PaymentFailed, x => x.CorrelateById(ctx => ctx.Message.SagaId));
            Event(() => StockReleased, x => x.CorrelateById(ctx => ctx.Message.SagaId));

            // Initially handles messages when saga is not created yet
            // when order is subbmited update state to reserving stock
            Initially(
                When(OrderSubmitted)
                    .Then(ctx =>
                    {
                        ctx.Saga.OrderId = ctx.Message.OrderId;
                        ctx.Saga.CreatedAt = DateTimeOffset.UtcNow;
                    })
                    .Publish(ctx => new StockReservedEvent(ctx.Saga.CorrelationId, ctx.Message.OrderId))
                    .TransitionTo(StockReserving)
            );

            // if stock reserver update state to payment charging
            During(StockReserving,
                When(StockReserved)
                    .Publish(ctx => new ChargePaymentCommand(ctx.Saga.CorrelationId, ctx.Saga.OrderId, 10, "Email@gmail.com"))
                    .TransitionTo(PaymentCharging),
                // if stock reservation failled finalize instance
                When(StockReservationFailed)
                    .TransitionTo(Cancelled)
                    .Finalize()
            );
            // if payment charged sucesfully, finalize instance, this is final state
            During(PaymentCharging, 
                When(PaymentCharged)
                    .Publish(ctx => new OrderCompletedCommand(ctx.Saga.OrderId, ctx.Saga.ConsumerEmail))
                    .TransitionTo(Completed)
                    .Finalize(),
                // if payment failed release stock
                When(PaymentFailed)
                    .Publish(ctx => new ReleaseStockCommand(ctx.Saga.CorrelationId, ctx.Saga.OrderId))
                    .TransitionTo(Compensating)
            );
            // if compensated succeeded finalize instance
            During(Compensating,
                When(StockReleased)
                    .Publish(ctx => new OrderCancelledCommand(ctx.Saga.OrderId, ctx.Saga.ConsumerEmail))
                    .TransitionTo(Compensated)
                    .Finalize()
            );

            // remove completed saga rows from db
            SetCompletedWhenFinalized();
        }
    }
}
