namespace RabbitMq.Contracts.Events
{
    public record OrderSubmitted(Guid OrderId, string ConsumerEmail, decimal Amount);
}
