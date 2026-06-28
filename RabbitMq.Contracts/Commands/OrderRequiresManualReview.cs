namespace RabbitMq.Contracts.Commands
{
    public record OrderRequiresManualReview(Guid OrderId, string CustomerEmail, string FailureReason);
}
