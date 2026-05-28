using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();

using var channel = await connection.CreateChannelAsync();

var arguments = new Dictionary<string, object>
{
    { "x-dead-letter-exchange", "orders.dlx" },
    { "x-message-ttl", 30000 }
};


await channel.QueueDeclareAsync("orders.queue", durable: true, exclusive: false, autoDelete: false, arguments);

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine($"Received {message}");

    await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
};

await channel.BasicConsumeAsync(queue: "orders.queue", autoAck: false, consumer);

Console.ReadLine();