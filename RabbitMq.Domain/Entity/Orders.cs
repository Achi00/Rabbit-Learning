namespace RabbitMq.Domain.Entity
{
    public sealed class Orders
    {
        public Guid Id { get; init; }
        public decimal Amount { get; init; }
        public string CustomerEmail { get; init; }
    }
}
