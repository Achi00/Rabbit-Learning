using RabbitMQ.Application.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();

using var channel = await connection.CreateChannelAsync();

//await TopologySetup.SetupTopologyAsync(channel);

Random rand = new Random();

for (int i = 1; i <= 10; i++)
{
    var order = new OrderMessage
    {
        Id = i,
        CustomerEmail = $"john{i}@example.com",
        Amount = 199.99m + (i * rand.Next(1, 100))
    };

    var json = JsonSerializer.Serialize(order);

    await channel.BasicPublishAsync(exchange: "orders.exchange", routingKey: "orders", body: Encoding.UTF8.GetBytes(json));

    Console.WriteLine($"sent Order with id: {i}");

    await Task.Delay(2000);
}


Console.ReadLine();

//// queues
//await channel.QueueDeclareAsync(
//    queue: "work-queue-v2",
//    durable: true,
//    exclusive: false,
//    autoDelete: false,
//    arguments: workQueueArgs);
//// dlx queue
//await channel.QueueDeclareAsync(
//    queue: "dead-letter-queue",
//    durable: true,
//    exclusive: false,
//    autoDelete: false,
//    arguments: null);
//// retry queue
//await channel.QueueDeclareAsync(
//    queue: "retry-queue",
//    durable: true,
//    exclusive: false,
//    autoDelete: false,
//    arguments: retryQueueArgs);

//// bindings
//await channel.QueueBindAsync(
//    queue: "work-queue-v2",
//    exchange: "work-exchange",
//    routingKey: "work");

//await channel.QueueBindAsync(
//    queue: "retry-queue",
//    exchange: "retry-exchange",
//    routingKey: string.Empty);
//// dlx

//await channel.QueueBindAsync(
//    queue: "dead-letter-queue",
//    exchange: "dead-letter-exchange",
//    routingKey: string.Empty);