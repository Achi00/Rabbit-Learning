using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();

using var channel = await connection.CreateChannelAsync();


var workQueueArgs = new Dictionary<string, object>
{
    { "x-dead-letter-exchange", "dead-letter-exchange" }
};

await channel.ExchangeDeclareAsync(
    "work-exchange", 
    ExchangeType.Direct,
    true, 
    false, 
    null);

await channel.QueueDeclareAsync(
    queue: "work-queue",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: workQueueArgs);

Console.WriteLine("Waiting for messages");

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (model, ea) =>
{
    try
    {
        Console.WriteLine("MESSAGE RECEIVED");
        throw new Exception("TESTING DLQ");
    }
    catch
    {
        await channel.BasicNackAsync(ea.DeliveryTag, false, false);
        Console.WriteLine($"Catching NACK {ea.DeliveryTag}");
    }
    //await Task.Delay(Random.Shared.Next(2000, 10000));
    //var body = ea.Body.ToArray();
    //var message = Encoding.UTF8.GetString(body);
    //Console.WriteLine($"Received message {message}");

    //await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
};

await channel.BasicQosAsync(
    prefetchSize: 0,
    prefetchCount: 1,
    global: false);

await channel.BasicConsumeAsync(
    queue: "work-queue", 
    autoAck: false,
    consumer: consumer);

Console.ReadLine();