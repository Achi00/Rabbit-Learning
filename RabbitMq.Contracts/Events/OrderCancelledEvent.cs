namespace RabbitMq.Contracts.Events
{
    public record OrderCancelledEvent(Guid OrderId, string CustomerEmail);
}
