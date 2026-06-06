using System.Text.Json;

namespace RabbitMQ.Application.Infrastructure.Envelope
{
    // decide what type name is, will not be depentend on .NET internals and namespaces
    public record MessageEnvelope
    {
        public Guid MessageId { get; init; } = Guid.NewGuid();
        // includes type only not full namespace
        public string MessageType { get; init; }
        // raw json untill type is unknown
        public JsonElement Payload { get; init; }
    }
}
