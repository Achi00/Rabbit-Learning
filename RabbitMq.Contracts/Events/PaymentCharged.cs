namespace RabbitMq.Contracts.Events
{
    public record PaymentCharged(Guid SagaId, Guid OrderId);

}
