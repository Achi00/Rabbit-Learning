namespace RabbitMq.Domain.Entity
{
    public record Order
    {
        public Guid Id { get; init; }
        public decimal Amount { get; init; }
        public string CustomerEmail { get; init; } = "";
    }
}
