using MassTransit;

namespace RabbitMq.Infrastructure.Messaging.Saga
{
    public class OrderSagaState : SagaStateMachineInstance
    {
        // links all messages belonging to same saga instance
        public Guid CorrelationId { get; set; }

        // masstransig tracks current state as string
        public string CurrentState { get; set; }

        // my order fields
        public Guid OrderId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public decimal Amount { get; set; }
        public string ConsumerEmail { get; set; } = string.Empty;
    }
}
