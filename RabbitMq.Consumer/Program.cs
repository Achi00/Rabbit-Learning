using RabbitMQ.Client;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();

using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(queue: "message", durable: true, exclusive: false, autoDelete: false, null);