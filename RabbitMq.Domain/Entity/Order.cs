namespace RabbitMq.Domain.Entity
{
    public sealed class Order
    {
        public Guid Id { get; init; }
        public decimal Amount { get; init; }
        public string CustomerEmail { get; init; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
