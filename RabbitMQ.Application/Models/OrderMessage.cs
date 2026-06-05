namespace RabbitMQ.Application.Models
{
    public sealed class OrderMessage
    {
        public Guid Id { get; init; }
        public decimal Amount { get; init; }
        public string CustomerEmail { get; init; }
    }
}
