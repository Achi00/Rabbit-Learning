namespace RabbitMq.Domain.Entity
{
    public sealed class OutboxMessage
    {
        public Guid Id { get; init; }
        public string MessageType { get; init; }
        public string Payload { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset? SentAt { get; set; }
    }
}
