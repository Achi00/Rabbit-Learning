using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Application.Infrastructure;
using RabbitMQ.Application.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace RabbitMQ.Application.Workers
{
    // this is part of producer side retry and will be its own seperated from consumer base queue retries
    // order producer with separated retry logic
    public class OrderProducerWorker : BackgroundService
    {
        private readonly RabbitMqConnectionProvider _connectionProvider;
        private readonly ILogger<OrderProducerWorker> _logger;

        public OrderProducerWorker(RabbitMqConnectionProvider connectionProvider, ILogger<OrderProducerWorker> logger)
        {
            _connectionProvider = connectionProvider;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            // channel options, activate publisher confirms
            var channelOptions = new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true
            );

            await using var channel = await _connectionProvider.Connection.CreateChannelAsync(channelOptions);

            var order = new OrderMessage
            {
                Id = Guid.NewGuid(),
                Amount = 0,
                CustomerEmail = "mail@gmail.com"
            };

            while (!ct.IsCancellationRequested)
            {
                await PublishWithRetryAsync(channel, JsonSerializer.Serialize(order), ct);
                await Task.Delay(2000, ct);
            }
        }

        // helper
        private async Task PublishWithRetryAsync(IChannel channel, string message, CancellationToken ct)
        {
            var body = Encoding.UTF8.GetBytes(message);
            var attempts = 0;
            const int maxAttempts = 3;

            while (attempts < maxAttempts)
            {
                try
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    cts.CancelAfter(TimeSpan.FromSeconds(5));
                    // after introduced channelOptions in this specs this will wait for broker confirmation before returning
                    await channel.BasicPublishAsync(
                        exchange: "orders.exchange", 
                        routingKey: "orders", 
                        mandatory: false,
                        basicProperties: new BasicProperties { Persistent = true },
                        body: body, 
                        cancellationToken: cts.Token
                    );
                    _logger.LogInformation("Confirmed: {message}", message);
                    return;
                }
                // only if operation cancelation is caused by token
                catch (OperationCanceledException) when (!ct.IsCancellationRequested)
                {
                    attempts++;
                    _logger.LogWarning("Connection timeout on attempt {Attempt} for {Message}", attempts, message);
                    await Task.Delay(attempts * 2000, ct);
                }
            }
            _logger.LogError("Failed to confirm {Message} after {Max} attempts", message, maxAttempts);
        }
    }
}
