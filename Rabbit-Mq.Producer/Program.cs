using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();

using var channel = await connection.CreateChannelAsync();

var arguments = new Dictionary<string, object>
{
    { "x-dead-letter-exchange", "orders.dlx" },
    { "x-message-ttl", 30000 }
};

try
{
    await channel.QueueDeleteAsync("orders.queue");
    await channel.QueueDeleteAsync("orders.dlq");
    await channel.QueueDeleteAsync("work-queue-v2");
    await channel.QueueDeleteAsync("retry-queue");
    await channel.QueueDeleteAsync("dead-letter-queue");

    await channel.ExchangeDeleteAsync("orders.exchange");
    await channel.ExchangeDeleteAsync("orders.dlx");
    await channel.ExchangeDeleteAsync("work-exchange");
    await channel.ExchangeDeleteAsync("retry-exchange");
    await channel.ExchangeDeleteAsync("dead-letter-exchange");

    Console.WriteLine("Cleaning...");
}
catch { }

await channel.QueueDeclareAsync(
    queue: "orders.queue",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: arguments
);

await channel.QueueDeclareAsync(
    "orders.dlq", 
    durable: true, 
    exclusive: false, 
    autoDelete: false
);
// Declare the DL exchange and bind the DLQ to it
await channel.ExchangeDeclareAsync("orders.dlx", ExchangeType.Fanout, durable: true);
await channel.ExchangeDeclareAsync("orders.exchange", ExchangeType.Direct, durable: true);



await channel.QueueBindAsync(queue: "orders.queue", exchange: "orders.exchange", routingKey: "orders");
await channel.QueueBindAsync(queue: "orders.dlq", exchange: "orders.dlx", routingKey: string.Empty);



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