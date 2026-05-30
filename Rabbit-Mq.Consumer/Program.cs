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

    // in case we passed stated retry counts
    if (waitQueue is null)
    {
        Console.WriteLine("Message failed permanently, dropping message");
        // ack message, drop it
        //await channel.BasicAckAsync(ea.DeliveryTag, false);
        // instead of ack message we send it to poison queue in case of future diagnosis
        // Grab the original death reason RabbitMQ stamps on every dead-lettered message
        var xDeath = ea.BasicProperties.Headers?.TryGetValue("x-death", out var d) == true
            ? d as List<object>
            : null;

        var firstDeath = xDeath?.FirstOrDefault() as Dictionary<string, object>;

        var originalQueue = firstDeath?.TryGetValue("queue", out var q) == true ? q?.ToString() : "unknown";

        var reason = firstDeath?.TryGetValue("reason", out var r) == true ? r?.ToString() : "unknown";

        var retryProps = new BasicProperties
        {
            Persistent = true,
            Headers = new Dictionary<string, object?>
            {
                { "x-retry-count", retryCount },
                { "x-original-queue", originalQueue },
                { "x-fail-reason", reason},
                { "x-failed-at", DateTimeOffset.UtcNow.ToString("O") },
            }
        };

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: "orders.poison",
            mandatory: false,
            basicProperties: retryProps,
            body: ea.Body.ToArray()
        );

        await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, false);
        return;
    }

    Console.WriteLine($"Retrying (attempt {retryCount + 1})...");

    // update headers
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
        exchange: "",
        routingKey: waitQueue,
        mandatory: false,
        basicProperties: props,
        body: ea.Body.ToArray()
    );

    await channel.BasicAckAsync(ea.DeliveryTag, false);

    await Task.Delay(500);
};

await channel.BasicConsumeAsync(queue: "orders.queue", autoAck: false, consumer);

Console.ReadLine();