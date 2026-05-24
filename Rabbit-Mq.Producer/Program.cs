using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();

using var channel = await connection.CreateChannelAsync();

// create exchange
await channel.ExchangeDeclareAsync("work-exchange", ExchangeType.Direct, true, false, null);
// queues
await channel.QueueDeclareAsync(queue: "work-queue", durable: true, exclusive: false, autoDelete: false, null);

// bindings
await channel.QueueBindAsync(queue: "work-queue", exchange: "work-exchange", routingKey: "work", arguments: null);


for (int i = 1; i <= 20; i++)
{
    var message = Encoding.UTF8.GetBytes($"Job #{i}");


    await channel.BasicPublishAsync(exchange: "work-exchange", routingKey: "work", body: message);
    
    Console.WriteLine($"sent: {i} - {Guid.NewGuid}");

    await Task.Delay(2000);
}


Console.ReadLine();