using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();

using var channel = await connection.CreateChannelAsync();

var dlqArgs = new Dictionary<string, object> 
{
    { "x-dead-letter-exchange", "my-dlx" },
    { "x-dead-letter-routing-key", "dead-letter-key" }
};

// create exchange
await channel.ExchangeDeclareAsync("work-exchange", ExchangeType.Direct, true, false, null);
// dead letter exchange
await channel.ExchangeDeclareAsync("dead-letter-exchange", ExchangeType.Fanout, true, false, null);

// queues
await channel.QueueDeclareAsync(queue: "work-queue", durable: true, exclusive: false, autoDelete: false, null);
// dlx queue
await channel.QueueDeclareAsync(queue: "dead-letter-queue", durable: true, exclusive: false, autoDelete: false, arguments: dlqArgs);


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