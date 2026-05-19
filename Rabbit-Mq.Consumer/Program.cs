using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();

using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(queue: "email-queue", durable: true, exclusive: false, autoDelete: false, null);
await channel.QueueDeclareAsync(queue: "sms-queue", durable: true, exclusive: false, autoDelete: false, null);
await channel.QueueDeclareAsync(queue: "push-queue", durable: true, exclusive: false, autoDelete: false, null);

Console.WriteLine("Waiting messages");

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (sender, eventArgs) =>
{
    var body = eventArgs.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine($"Received: {message}");

    // ACK
    await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
};

// manual ackgnowladge messages
await channel.BasicConsumeAsync("email-queue", autoAck: false, consumer);
await channel.BasicConsumeAsync("sms-queue", autoAck: false, consumer);
await channel.BasicConsumeAsync("push-queue", autoAck: false, consumer);

Console.ReadLine();