using MassTransit;
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
        public Event<StockReservedEvent> StockReserved { get; private set; }
        public Event<StockReservationFailedEvent> StockReservationFailed { get; private set; }
        public Event<PaymentChargedEvent> PaymentCharged { get; private set; }
        public Event<PaymentFailedEvent> PaymentFailed { get; private set; }
        public Event<StockReleasedEvent> StockReleased { get; private set; }
    }
}
