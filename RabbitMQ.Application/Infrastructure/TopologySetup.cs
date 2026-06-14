using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace RabbitMQ.Application.Infrastructure
{
    // change to IHostedService? only will run on statup
    public class TopologySetup : IHostedService
    {
        private readonly RabbitMqConnectionProvider _provider;

        public TopologySetup(RabbitMqConnectionProvider provider)
        {
            _provider = provider;
        }
        public async Task StartAsync(CancellationToken ct)
        {
            using var channel = await _provider.Connection.CreateChannelAsync();

            // using new topic exchanges instead of direct
            /*
             old - await channel.ExchangeDeclareAsync("orders.exchange", ExchangeType.Direct, durable: true);
            */
            // introduced new topic exchange
            await channel.ExchangeDeclareAsync("orders.exchange", ExchangeType.Topic, durable: true);

            await channel.QueueBindAsync("orders.queue", "orders.exchange", routingKey: "order.*");

            await channel.ExchangeDeclareAsync("orders.dlx", ExchangeType.Fanout, durable: true);

            var queueArgs = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", "orders.dlx" }
            };

            await channel.QueueDeclareAsync("orders.queue", durable: true, exclusive: false, autoDelete: false, arguments: queueArgs);
            await channel.QueueDeclareAsync("orders.dlq", durable: true, exclusive: false, autoDelete: false);
            await channel.QueueDeclareAsync("orders.poison", durable: true, exclusive: false, autoDelete: false);

            foreach (var delay in new[] { 5_000, 30_000, 300_000 })
            {
                var retryArgs = new Dictionary<string, object>
            {
                { "x-message-ttl", delay },
                { "x-dead-letter-exchange", "orders.exchange" },
                { "x-dead-letter-routing-key", "orders" }
            };
                await channel.QueueDeclareAsync($"orders.retry.{delay / 1000}s", durable: true, exclusive: false, autoDelete: false, arguments: retryArgs);
            }

            await channel.QueueBindAsync("orders.queue", "orders.exchange", routingKey: "orders");
            await channel.QueueBindAsync("orders.dlq", "orders.dlx", routingKey: string.Empty);
        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
