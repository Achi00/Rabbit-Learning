using System.Text.Json;

namespace RabbitMQ.Application.Infrastructure.Envelope
{
    // decide what type name is, will not be depentend on .NET internals and namespaces
    public record MessageEnvelope
    {
        public Guid MessageId { get; init; } = Guid.NewGuid();
        public string MessageType { get; init; }
        public JsonElement Payload { get; init; }
    }
}
