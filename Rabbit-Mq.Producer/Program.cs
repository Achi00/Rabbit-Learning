using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();

using var channel = await connection.CreateChannelAsync();

var workQueueArgs = new Dictionary<string, object>
{
    { "x-dead-letter-exchange", "dead-letter-exchange" },
};


// create exchange
await channel.ExchangeDeclareAsync(
    exchange: "work-exchange",
    type: ExchangeType.Direct,
    durable: true);

await channel.ExchangeDeclareAsync(
    exchange: "dead-letter-exchange",
    type: ExchangeType.Fanout,
    durable: true);


// queues
await channel.QueueDeclareAsync(
    queue: "work-queue",
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

// bindings
await channel.QueueBindAsync(
    queue: "work-queue",
    exchange: "work-exchange",
    routingKey: string.Empty);
// dlx

await channel.QueueBindAsync(
    queue: "dead-letter-queue",
    exchange: "dead-letter-exchange",
    routingKey: "dead-letter-key");


for (int i = 1; i <= 20; i++)
{
    var message = Encoding.UTF8.GetBytes($"Job #{i}");


    await channel.BasicPublishAsync(exchange: "work-exchange", routingKey: "work", body: message);
    
    Console.WriteLine($"sent: {i} - {Guid.NewGuid}");

    await Task.Delay(2000);
}


Console.ReadLine();