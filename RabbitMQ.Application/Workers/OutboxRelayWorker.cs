using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Application.Infrastructure.Envelope;
using RabbitMQ.Application.Interfaces.Messages;
using RabbitMqDemo.Persistance.Context;
using System.Text.Json;

namespace RabbitMQ.Application.Workers
{
    public class OutboxRelayWorker : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<OutboxRelayWorker> _logger;

        public OutboxRelayWorker(IServiceProvider services, ILogger<OutboxRelayWorker> logger)
        {
            _services = services;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessPendingMessagesAsync(stoppingToken);
                // poll every 5 seconds
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        private async Task ProcessPendingMessagesAsync(CancellationToken ct)
        {
            await using var scope = _services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<MessageDbContext>();

            var publisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();
            // fetch unsent rows
            var pending = await db.OutboxMessages
                .Where(x => x.SentAt == null)
                .OrderBy(x => x.CreatedAt)
                .Take(50)
                .ToListAsync(ct);

            foreach (var message in pending)
            {
                try
                {
                    var envelope = new MessageEnvelope
                    {
                        MessageId = message.Id,
                        MessageType = message.MessageType,
                        Payload = JsonSerializer.Deserialize<JsonElement>(message.Payload)
                    };

                    await publisher.PublishAsync(envelope, ct);

                    // mark as sent
                    message.SentAt = DateTimeOffset.UtcNow;
                    await db.SaveChangesAsync(ct);

                    _logger.LogInformation("Relayed {MessageType} {MessageId}", message.MessageType, message.Id);
                }
                catch (Exception ex)
                {
                    // sentAt is null
                    _logger.LogWarning(ex, "Failed to relay {MessageId}, will retry", message.Id);
                }
            }
        }
    }
}
