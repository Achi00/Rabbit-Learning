using RabbitMQ.Application.Infrastructure;
using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();

using var channel = await connection.CreateChannelAsync();

await TopologySetup.SetupTopologyAsync(channel);


for (int i = 1; i <= 1; i++)
{
    var message = Encoding.UTF8.GetBytes($"Job #{i}");

    await channel.BasicPublishAsync(exchange: "orders.exchange", routingKey: "orders", body: message);

    Console.WriteLine($"sent: {i} - {Guid.NewGuid()}");

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