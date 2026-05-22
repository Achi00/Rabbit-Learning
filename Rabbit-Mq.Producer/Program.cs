using RabbitMQ.Client;
using System.Text;


var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();

using var channel = await connection.CreateChannelAsync();

// clear old messages with QueuePurgeAsync keeps: queues, binsings and consumers alive
//await channel.QueuePurgeAsync("email-queue");
//await channel.QueuePurgeAsync("sms-queue");
//await channel.QueuePurgeAsync("push-queue");
//Console.WriteLine("Removed old messages");

// create exchange
await channel.ExchangeDeclareAsync("work-exchange", ExchangeType.Direct, true, false, null);
// queues
await channel.QueueDeclareAsync(queue: "work-queue", durable: true, exclusive: false, autoDelete: false, null);

// bindings
await channel.QueueBindAsync(queue: "work-queue", exchange: "work-exchange", routingKey: "work", arguments: null);


for (int i = 1; i <= 20; i++)
{
    var message = Encoding.UTF8.GetBytes($"Job #{i}");

    Console.WriteLine($"message: #{i} was sent");

    await channel.BasicPublishAsync(exchange: "work-exchange", routingKey: "work", body: message);
}


Console.ReadLine();