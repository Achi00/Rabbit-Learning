using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();

using var channel = await connection.CreateChannelAsync();

var workQueueArgs = new Dictionary<string, object>
{
    { "x-dead-letter-exchange", "retry-exchange" }
    //{ "x-dead-letter-exchange", "dead-letter-exchange" }
};

var retryQueueArgs = new Dictionary<string, object>
{
    { "x-message-ttl", 10000 },
    { "x-dead-letter-exchange", "work-exchange" },
    { "x-dead-letter-routing-key", "work" }
};


// create exchange
await channel.ExchangeDeclareAsync(
    exchange: "work-exchange",
    type: ExchangeType.Direct,
    durable: true);

await channel.ExchangeDeclareAsync(
    "retry-exchange",
    ExchangeType.Direct,
    durable: true);

await channel.ExchangeDeclareAsync(
    exchange: "dead-letter-exchange",
    type: ExchangeType.Fanout,
    durable: true);


// queues
await channel.QueueDeclareAsync(
    queue: "work-queue-v2",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: workQueueArgs);
// dlx queue
await channel.QueueDeclareAsync(
    queue: "dead-letter-queue",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null);
// retry queue
await channel.QueueDeclareAsync(
    queue: "retry-queue",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: retryQueueArgs);

// bindings
await channel.QueueBindAsync(
    queue: "work-queue-v2",
    exchange: "work-exchange",
    routingKey: "work");

await channel.QueueBindAsync(
    queue: "retry-queue",
    exchange: "retry-exchange",
    routingKey: string.Empty);
// dlx

await channel.QueueBindAsync(
    queue: "dead-letter-queue",
    exchange: "dead-letter-exchange",
    routingKey: string.Empty);


for (int i = 1; i <= 20; i++)
{
    var message = Encoding.UTF8.GetBytes($"Job #{i}");


    await channel.BasicPublishAsync(exchange: "work-exchange", routingKey: "work", body: message);
    
    Console.WriteLine($"sent: {i} - {Guid.NewGuid()}");

    await Task.Delay(2000);
}


Console.ReadLine();