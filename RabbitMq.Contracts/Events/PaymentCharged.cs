namespace RabbitMq.Contracts.Events
{
    // uses CorrelationId for message state instance id matching
    public record PaymentCharged(Guid CorrelationId, Guid OrderId);

}
