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
    queue: "work-queue-v2",
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
        int retryCount = 0;

        if (ea.BasicProperties.Headers?.ContainsKey("x-death") == true)
        {
            var deaths = (List<object>)ea.BasicProperties.Headers["x-death"];

            var death = (Dictionary<string, object>)deaths[0];

            retryCount = Convert.ToInt32(death["count"]);
        }

        Console.WriteLine($"Retry count: {retryCount}");

        if (retryCount >= 3)
        {
            Console.WriteLine("sending to final DLQ");

            await channel.BasicPublishAsync("dead-letter-exchange", "", ea.Body);

            await channel.BasicAckAsync(ea.DeliveryTag, false);

            return;
        }
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