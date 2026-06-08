namespace RabbitMq.Domain.Entity
{
    // idempotency only
    public sealed class ProcessedMessages
    {
        public Guid MessageId { get; set; }
        public string MessageType { get; set; } = string.Empty;
        public DateTimeOffset ProcessedAt { get; set; }
    }
}
