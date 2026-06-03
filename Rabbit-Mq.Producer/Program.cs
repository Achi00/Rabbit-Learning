using RabbitMQ.Application.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();

// channel options, activate publisher confirms
var channelOptions = new CreateChannelOptions(
    publisherConfirmationsEnabled: true, 
    publisherConfirmationTrackingEnabled: true
);

using var channel = await connection.CreateChannelAsync(channelOptions);

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

    await channel.BasicPublishAsync(exchange: "orders.exchange", routingKey: "orders", body: Encoding.UTF8.GetBytes(json));

    Console.WriteLine($"sent Order with id: {i}");

    await Task.Delay(2000);
}


Console.ReadLine();