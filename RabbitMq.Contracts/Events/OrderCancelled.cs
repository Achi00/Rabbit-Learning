namespace RabbitMq.Contracts.Events
{
    public record OrderCancelled(Guid OrderId, string CustomerEmail, string? Reason);
}
