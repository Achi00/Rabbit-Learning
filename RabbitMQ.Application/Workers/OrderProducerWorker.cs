using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Application.Infrastructure;
using RabbitMQ.Client;
using System.Text;

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

            using var channel = await _connectionProvider.Connection.CreateChannelAsync(channelOptions);

            var jobNumber = 1;

            while (!ct.IsCancellationRequested)
            {
                await PublishWithRetryAsync(channel, $"Job #{jobNumber}", ct);
                jobNumber++;
                await Task.Delay(2000, ct);
            }
        }

        private async Task PublishWithRetryAsync(IChannel channel, string message, CancellationToken ct)
        {
            var body = Encoding.UTF8.GetBytes(message);
            var attempts = 0;
            const int maxAttempts = 3;

            while (attempts < maxAttempts)
            {
                attempts++;
                _logger.LogWarning("confirm timeout, attempt {Attempr}", attempts);
                await Task.Delay(TimeSpan.FromSeconds(attempts * 2));
            }
        }
    }
}
