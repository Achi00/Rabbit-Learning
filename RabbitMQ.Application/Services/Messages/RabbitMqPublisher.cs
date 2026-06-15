using Microsoft.Extensions.Logging;
using RabbitMQ.Application.Helpers;
using RabbitMQ.Application.Infrastructure;
using RabbitMQ.Application.Infrastructure.Envelope;
using RabbitMQ.Application.Interfaces.Messages;
using RabbitMQ.Client;

namespace RabbitMQ.Application.Services.Messages
{
    // introducting this should be more extendable version of publishing messages with messagerouter with dict key, instead of hard code exchange + routeing key
    // topic based routing instead of direct
    public class RabbitMqPublisher : IMessagePublisher
    {
        private readonly RabbitMqConnectionProvider _provider;
        private readonly ILogger<RabbitMqPublisher> _logger;

        public RabbitMqPublisher(RabbitMqConnectionProvider provider, ILogger<RabbitMqPublisher> logger)
        {
            _provider = provider;
            _logger = logger;
        }
        public async Task PublishAsync(MessageEnvelope envelope, CancellationToken ct = default)
        {
            // determins queue based on message type
            var (exchange, routingKey) = MessageRouting.Resolve(envelope.MessageType);

            var connection = await _provider.GetConnetionAsync();

            await using var channel = await connection.CreateChannelAsync
            (
                // channel options, activate publisher confirms
                new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true)    
            );

            var body = MessageSerializer.Serialize(envelope);

            var props = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                MessageId = envelope.MessageId.ToString(),
                Type = envelope.MessageType
            };

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            await channel.BasicPublishAsync(exchange, routingKey, false, props, body, cts.Token);

            _logger.LogInformation("Published {MessageId} to {Exchange}/{RouteKey}", envelope.MessageId, exchange, routingKey);
        }
    }
}
