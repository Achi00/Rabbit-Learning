namespace RabbitMq.Contracts.Events
{
    public record PaymentFailedEvent(Guid SagaId, Guid OrderId, string Reason);
}
