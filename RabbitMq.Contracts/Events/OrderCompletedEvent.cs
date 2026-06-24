namespace RabbitMq.Contracts.Events
{
    public record OrderCompletedEvent(Guid OrderId, string CustomerEmail);

}
