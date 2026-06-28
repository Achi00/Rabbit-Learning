namespace RabbitMq.Contracts.Commands
{
    public record OrderRequiresManualReview(Guid orderId, string customerEmail, string failureReason);
}
