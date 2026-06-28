using MassTransit;
using RabbitMq.Contracts.Commands;
using RabbitMq.Contracts.Events;

namespace RabbitMq.Infrastructure.Messaging.Saga
{
    /*
     * Changing sata state and ef core interaction happends on initialization after calling methon Then()
     * after TransitionTo, ef core should match correlaton id to saga id
     */

    /*
     * Connections between saga, consumers, events and commands
     * Saga receives event -> saga sends command -> consumer does work -> consumer publishes event -> saga receiver event
     */
    public class OrderStateMachine : MassTransitStateMachine<OrderSagaState>
    {
        // this states replace old enum SagaStep
        public State StockReserving { get; private set; }
        public State PaymentCharging { get; private set; }
        public State Completed { get; private set; }
        public State Cancelled { get; private set; }
        public State Compensating { get; private set; }
        public State Compensated { get; private set; }
        public State ManualReview { get; private set; } = null!;

        // events, each maps to message type
        public Event<OrderSubmitted> OrderSubmitted { get; private set; }
        public Event<OrderCompleted> OrderCompleted { get; private set; }
        public Event<OrderCancelled> OrderCancelled { get; private set; }
        public Event<StockReserved> StockReserved { get; private set; }
        public Event<StockReservationFailed> StockReservationFailed { get; private set; }
        public Event<PaymentCharged> PaymentCharged { get; private set; }
        public Event<PaymentFailed> PaymentFailed { get; private set; }
        public Event<StockReleased> StockReleased { get; private set; }
        public Event<StockReleaseFailed> StockReleaseFailed { get; private set; }
        public Event<ManualReviewCompleted> ManualReviewCompleted { get; private set; }

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
            Event(() => StockReleaseFailed, x => x.CorrelateById(ctx => ctx.Message.SagaId));
            Event(() => ManualReviewCompleted, x => x.CorrelateById(ctx => ctx.Message.SagaId));


            /*
             * Saga waiting and executing events, no async code here at the moment
             * Masstransir gets message, identifies message type, then gets correlation id, 
             * fetches row with ef core from db
             * checks current state, tryes to match in During/When process, 
             * updates state in memory, publishes message, updates db row
             * 
             * on every new process it fetches by correlation id and does same process again
             * based on db row state + event type passed in During/When
             */

            // Initially handles messages when saga is not created yet
            // when order is subbmited update state to reserving stock
            Initially(
                When(OrderSubmitted)
                    // this updates value which should go into OrderSagaStates table
                    .Then(ctx =>
                    {
                        ctx.Saga.OrderId = ctx.Message.OrderId;
                        ctx.Saga.CreatedAt = DateTimeOffset.UtcNow;
                        ctx.Saga.Amount = ctx.Message.Amount;
                        ctx.Saga.CustomerEmail = ctx.Message.CustomerEmail;
                    })
                    // publishes to broker
                    .Publish(ctx => new ReserveStock(ctx.Saga.CorrelationId, ctx.Message.OrderId))
                    // after transitioning to StockReserving ef core should insert values and update state as StockReserving
                    .TransitionTo(StockReserving)
            );

            // if stock reserver update state to payment charging
            During(StockReserving,
                When(StockReserved)
                    /*
                     * if Forcing delivery to specific queue is needed, for commands, will be using Send(). it is point to point and calls specific service,
                     * knows exact destination and has one logical receiving endpoint
                     * sends commands
                     */
                    .Publish(ctx => new ChargePayment(ctx.Saga.CorrelationId, ctx.Saga.OrderId, ctx.Saga.Amount, ctx.Saga.CustomerEmail))
                    .TransitionTo(PaymentCharging),
                // if stock reservation failled finalize instance
                When(StockReservationFailed)
                    .Then(ctx =>
                    {
                        ctx.Saga.FailureReason = ctx.Message.Reason;
                    })
                    .Publish(ctx => new OrderCancelled(ctx.Saga.OrderId, ctx.Saga.CustomerEmail, ctx.Saga.FailureReason))
                    .TransitionTo(Cancelled)
                    // finalize removes row???
                    .Finalize()
            );
            // if payment charged sucesfully, finalize instance, this is final state
            // no need for transitioning to Completed state if we finalized it
            During(PaymentCharging, 
                When(PaymentCharged)
                    .Publish(ctx => new OrderCompleted(ctx.Saga.OrderId, ctx.Saga.CustomerEmail))
                    .Finalize(),
                // if payment failed release stock
                When(PaymentFailed)
                    .Then(ctx =>
                    {
                        ctx.Saga.FailureReason = ctx.Message.Reason;
                    })
                    .Publish(ctx => new ReleaseStock(ctx.Saga.CorrelationId, ctx.Saga.OrderId))
                    .TransitionTo(Compensating)
            );
            // if compensated succeeded finalize instance
            During(Compensating,
                When(StockReleased)
                    .Publish(ctx => new OrderCancelled(ctx.Saga.OrderId, ctx.Saga.CustomerEmail, ctx.Saga.FailureReason ?? "Order failed compensating"))
                    .Finalize(),

                When(StockReleaseFailed)
                    .Then(ctx =>
                    {
                        ctx.Saga.FailureReason = $"Stock release failed: {ctx.Message.Reason}";
                    })
                    .Publish(ctx => new OrderRequiresManualReview(
                        ctx.Saga.OrderId,
                        ctx.Saga.CustomerEmail,
                        ctx.Saga.FailureReason ?? "Manual review required"
                    ))
                    // will require admin or any human interaction/review for resolving
                    // later admin will publish ManualReviewCompleted
                    .TransitionTo(ManualReview)
            );

            // if state enters manual revies step
            // unsolved workflow will stay visible
            During(ManualReview,
                When(ManualReviewCompleted)
                    .Finalize()
            );

            // remove completed saga rows from db
            SetCompletedWhenFinalized();
        }
    }
}
