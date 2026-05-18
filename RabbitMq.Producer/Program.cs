using RabbitMQ.Client;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();

using var channel = await connection.CreateChannelAsync();

// create exchange
await channel.ExchangeDeclareAsync("app-exchange", ExchangeType.Direct, true, false, null);
// queues
await channel.QueueDeclareAsync(queue: "email-queue", durable: true, exclusive: false, autoDelete: false, null);
await channel.QueueDeclareAsync(queue: "sms-queue", durable: true, exclusive: false, autoDelete: false, null);
await channel.QueueDeclareAsync(queue: "push-queue", durable: true, exclusive: false, autoDelete: false, null);

// bindings
await channel.QueueBindAsync(queue: "email-queue", exchange: "app-exchange", routingKey: "email");
await channel.QueueBindAsync(queue: "sms-queue", exchange: "app-exchange", routingKey: "sms");
await channel.QueueBindAsync(queue: "push-queue", exchange: "app-exchange", routingKey: "push");
