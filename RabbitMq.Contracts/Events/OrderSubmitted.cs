namespace RabbitMq.Contracts.Events
{
    public record OrderSubmitted(Guid OrderId, string CustomerEmail, decimal Amount);
}
