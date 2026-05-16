using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();

using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(queue: "message", durable: true, exclusive: false, autoDelete: false, null);

Console.WriteLine("Waiting messages");

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (sender, eventArgs) => 
{ 
    var body = eventArgs.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine($"Received: {message}");

    // ACK
    await ((AsyncEventingBasicConsumer)sender).Channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
};

// manual ackgnowladge messages
await channel.BasicConsumeAsync("message", autoAck: false, consumer);

Console.ReadLine();