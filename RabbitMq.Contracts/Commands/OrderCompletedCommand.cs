namespace RabbitMq.Contracts.Commands
{
    public record OrderCompletedCommand(Guid OrderId, string CustomerEmail);
}
