namespace RabbitMq.Contracts.Events
{
    public record PaymentChargedEvent(Guid SagaId, Guid OrderId);

}
