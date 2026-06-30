namespace RabbitMq.Contracts.Events
{
    public record ManualReviewCompleted(Guid CorrelationId, Guid OrderId);
}
