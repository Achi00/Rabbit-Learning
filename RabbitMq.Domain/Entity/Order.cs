using RabbitMq.Domain.Enums;

namespace RabbitMq.Domain.Entity
{
    public class Order
    {
        public Guid Id { get; set; }

        public decimal Amount { get; set; }

        public string CustomerEmail { get; set; } = string.Empty;

        public OrderStatus Status { get; set; }

        public string? FailureReason { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? CompletedAt { get; set; }

        public DateTimeOffset? CancelledAt { get; set; }
    }
}
