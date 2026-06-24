namespace RabbitMq.Contracts.Events
{
    public record PaymentFailed(Guid SagaId, Guid OrderId, string Reason);
}
