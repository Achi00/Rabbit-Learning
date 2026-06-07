using System.Collections.Concurrent;

namespace RabbitMQ.Application.Services.Messages
{
    // simple in memory idempotacny checks
    public class InMemoryIdempotencyService
    {
        private readonly ConcurrentDictionary<Guid, DateTimeOffset> _processed = new();

        public bool IsDuplicate(Guid messageId) => _processed.ContainsKey(messageId);

        public void MarkAsProcessed(Guid messageId) => _processed.TryAdd(messageId, DateTimeOffset.UtcNow);
    }
}
