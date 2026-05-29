using RabbitMQ.Application;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await Setup.SetupTopologyAsync(channel);

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (model, ea) =>
{
    var headers = ea.BasicProperties.Headers ?? new Dictionary<string, object>()!;

    var retryCount = headers.TryGetValue("x-retry-count", out var val) ? Convert.ToInt32(val) : 0;

    // set up in SetupTopologyAsync
    var waitQueue = retryCount switch
    {
        0 => "orders.retry.5s",
        1 => "orders.retry.30s",
        2 => "orders.retry.300s",
        _ => null
    };

    if (waitQueue is null)
    {
        Console.WriteLine("Message failed permanently, dropping message");
        // ack message, drop it
        await channel.BasicAckAsync(ea.DeliveryTag, false);
        return;
    }

    Console.WriteLine($"Retrying (attempt {retryCount + 1})...");

    // uodate headers
    var props = new BasicProperties
    {
        Persistent = true,
        Headers = new Dictionary<string, object>()
        {
            { "x-retry-count", retryCount + 1 }
        }!
    };

    // republish back to main exchange
    await channel.BasicPublishAsync(
        exchange: "orders.exchange",
        routingKey: "orders",
        mandatory: false,
        basicProperties: props,
        body: ea.Body.ToArray()
    );

    await channel.BasicAckAsync(ea.DeliveryTag, false);

    await Task.Delay(500);
};

await channel.BasicConsumeAsync(queue: "orders.queue", autoAck: false, consumer);

Console.ReadLine();