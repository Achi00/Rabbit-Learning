namespace RabbitMQ.Application.Models
{
    public sealed class OrderMessage
    {
        public int Id { get; init; }
        public decimal Amount { get; init; }
        public string CustomerEmail { get; init; }
    }
}
