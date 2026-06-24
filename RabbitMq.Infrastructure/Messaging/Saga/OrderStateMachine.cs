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
        public Event<OrderSubmitted> OrderSubmitted { get; private set; }
        public Event<OrderCompleted> OrderCompleted { get; private set; }
        public Event<OrderCancelled> OrderCancelled { get; private set; }
        public Event<StockReserved> StockReserved { get; private set; }
        public Event<StockReservationFailed> StockReservationFailed { get; private set; }
        public Event<PaymentCharged> PaymentCharged { get; private set; }
        public Event<PaymentFailed> PaymentFailed { get; private set; }
        public Event<StockReleased> StockReleased { get; private set; }

        public OrderStateMachine()
        {
            // current state string
            InstanceState(x => x.CurrentState);

            // correlate incoming messages to correct saga instance
            Event(() => OrderSubmitted, x =>
            {
                x.CorrelateById(context => context.Message.OrderId);

                x.SelectId(context => context.Message.OrderId);
            });

            // after every new arrivel masstransit reads correlation and should load saga from db
            Event(() => StockReserved, x => x.CorrelateById(ctx => ctx.Message.SagaId));
            Event(() => StockReservationFailed, x => x.CorrelateById(ctx => ctx.Message.SagaId));
            Event(() => PaymentCharged, x => x.CorrelateById(ctx => ctx.Message.SagaId));
            Event(() => PaymentFailed, x => x.CorrelateById(ctx => ctx.Message.SagaId));
            Event(() => StockReleased, x => x.CorrelateById(ctx => ctx.Message.SagaId));

            // Initially handles messages when saga is not created yet
            // when order is subbmited update state to reserving stock
            Initially(
                When(OrderSubmitted)
                    // this updates value which should go into OrderSagaStates table
                    .Then(ctx =>
                    {
                        ctx.Saga.OrderId = ctx.Message.OrderId;
                        ctx.Saga.CreatedAt = DateTimeOffset.UtcNow;
                    })
                    // publishes to broker
                    .Publish(ctx => new ReserveStock(ctx.Saga.CorrelationId, ctx.Message.OrderId))
                    // after transitioning to StockReserving ef core should insert values and update state as StockReserving
                    .TransitionTo(StockReserving)
            );

            // if stock reserver update state to payment charging
            During(StockReserving,
                When(StockReserved)
                    .Publish(ctx => new ChargePayment(ctx.Saga.CorrelationId, ctx.Saga.OrderId, 10, "Email@gmail.com"))
                    .TransitionTo(PaymentCharging),
                // if stock reservation failled finalize instance
                When(StockReservationFailed)
                    .TransitionTo(Cancelled)
                    // finalize removes row???
                    .Finalize()
            );
            // if payment charged sucesfully, finalize instance, this is final state
            During(PaymentCharging, 
                When(PaymentCharged)
                    .Publish(ctx => new OrderCompleted(ctx.Saga.OrderId, ctx.Saga.ConsumerEmail))
                    .TransitionTo(Completed)
                    .Finalize(),
                // if payment failed release stock
                When(PaymentFailed)
                    .Publish(ctx => new ReleaseStock(ctx.Saga.CorrelationId, ctx.Saga.OrderId))
                    .TransitionTo(Compensating)
            );
            // if compensated succeeded finalize instance
            During(Compensating,
                When(StockReleased)
                    .Publish(ctx => new OrderCancelled(ctx.Saga.OrderId, ctx.Saga.ConsumerEmail))
                    .TransitionTo(Compensated)
                    .Finalize()
            );

            // remove completed saga rows from db
            //SetCompletedWhenFinalized();
        }
    }
}
