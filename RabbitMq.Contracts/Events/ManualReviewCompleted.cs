namespace RabbitMq.Contracts.Events
{
    public record ManualReviewCompleted(Guid SagaId, Guid OrderId);
}
