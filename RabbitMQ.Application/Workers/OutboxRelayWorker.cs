using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMqDemo.Persistance.Context;

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
                //await P
            }
        }

        private async Task ProcessPendingMessagesAsync(CancellationToken ct)
        {
            await using var scope = _services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<MessageDbContext>();
        }
    }
}
