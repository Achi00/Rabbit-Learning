using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;


Console.WriteLine("Press to start");
Console.ReadLine();

await Task.Delay(5000);

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();

using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync("work-exchange", ExchangeType.Direct, true, false, null);

await channel.QueueDeclareAsync(queue: "work-queue", durable: true, exclusive: false, autoDelete: false, null);

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"Received message {message}");

    await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
};

await channel.BasicConsumeAsync(queue: "work-queue", autoAck: false, consumer: consumer);

Console.ReadLine();