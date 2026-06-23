namespace RabbitMq.Contracts.Commands
{
    public record OrderCancelledCommand(Guid OrderId, string CustomerEmail);
}
