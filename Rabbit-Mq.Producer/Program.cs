using RabbitMQ.Application.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();





Random rand = new Random();

for (int i = 1; i <= 10; i++)
{
    var order = new OrderMessage
    {
        Id = i,
        CustomerEmail = $"john{i}@example.com",
        Amount = 199.99m + (i * rand.Next(1, 100))
    };

    var json = JsonSerializer.Serialize(order);

    var message = Encoding.UTF8.GetBytes(json);

    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

    try
    {
        // after introduced channelOptions in this specs this will wait for broker confirmation before returning
        await channel.BasicPublishAsync(exchange: "orders.exchange", routingKey: "orders", mandatory: false, body: message, cancellationToken: cts.Token);
    }
    catch (OperationCanceledException)
    {
        // broker did not confirm within 5 seconds, 5 = cts passed valure
        // safe to retry publish message may arrive?
        Console.WriteLine("publisher confirmation timed out, retrying...");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex.Message, "Publish failed");
    }

    Console.WriteLine($"sent Order with id: {i}");
    Console.WriteLine("broker confirmed message persisted");

    await Task.Delay(2000);
}


Console.ReadLine();