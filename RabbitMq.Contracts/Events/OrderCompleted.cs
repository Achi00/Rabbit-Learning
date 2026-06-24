namespace RabbitMq.Contracts.Commands
{
    public record OrderCompleted(Guid OrderId, string CustomerEmail);
}
