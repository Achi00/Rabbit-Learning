namespace RabbitMq.Contracts.Events
{
    public record OrderCompleted(Guid OrderId, string CustomerEmail);

}
