using RabbitMQ.Application.Infrastructure.Envelope;

namespace RabbitMQ.Application.Interfaces.Messages
{
    public interface IMessagePublisher
    {
        Task PublishAsync(MessageEnvelope envelope, CancellationToken ct = default);
    }
}
