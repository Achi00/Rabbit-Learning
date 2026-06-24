namespace RabbitMq.Contracts.Commands
{
    public record OrderCancelled(Guid OrderId, string CustomerEmail);
}
