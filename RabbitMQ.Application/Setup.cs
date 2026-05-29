using RabbitMQ.Client;

namespace RabbitMQ.Application
{
    public static class Setup
    {
        public static async Task SetupTopologyAsync(IChannel channel)
        {
            // delays in ms
            var delays = new[] { 5_000, 30_000, 300_000 };

            // new retry queue pass delay values, if fail back to dlx
            foreach (var delay in delays)
            {
                var retryArgs = new Dictionary<string, object>
                {
                    { "x-message-ttl", delay },
                    { "x-dead-letter-exchange", "orders.exchange" },
                    { "x-dead-letter-routing-key", "orders" }
                };

                await channel.QueueDeclareAsync(
                    queue: $"orders.retry.{delay / 1000}s",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: retryArgs!
                );
            }

            var dlqArgs = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", "orders.dlx" },
                { "x-message-ttl", 30000 }
            };

            await channel.ExchangeDeclareAsync("orders.exchange", ExchangeType.Direct, durable: true);
            await channel.ExchangeDeclareAsync("orders.dlx", ExchangeType.Fanout, durable: true);

            await channel.QueueDeclareAsync("orders.queue", durable: true, exclusive: false, autoDelete: false, arguments: dlqArgs);
            await channel.QueueDeclareAsync("orders.dlq", durable: true, exclusive: false, autoDelete: false);

            await channel.QueueBindAsync("orders.queue", "orders.exchange", routingKey: "orders");
            await channel.QueueBindAsync("orders.dlq", "orders.dlx", routingKey: string.Empty);
        }
    }
}
