using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Application.Infrastructure;
using RabbitMQ.Application.Workers;
using RabbitMQ.Client;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();

var builder = Host.CreateApplicationBuilder(args);

var connectionProvider = await RabbitMqConnectionProvider.CreateAsync("localhost");
builder.Services.AddSingleton(connectionProvider);
// worker
builder.Services.AddHostedService<OrderProducerWorker>();

var host = builder.Build();

await host.RunAsync();