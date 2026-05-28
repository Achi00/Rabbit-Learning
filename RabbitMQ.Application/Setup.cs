using RabbitMQ.Client;

namespace RabbitMQ.Application
{
    public static class Setup
    {
        public static async Task SetupTopologyAsync(IChannel channel)
        {
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
