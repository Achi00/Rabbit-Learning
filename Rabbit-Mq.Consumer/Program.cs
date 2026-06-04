using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Application.Infrastructure;
using RabbitMQ.Application.Workers;

var builder = Host.CreateApplicationBuilder(args);

var connectionProvider = await RabbitMqConnectionProvider.CreateAsync("localhost");
builder.Services.AddSingleton(connectionProvider);

// Workers
builder.Services.AddHostedService<TopologySetup>();
builder.Services.AddHostedService<OrderConsumerWorker>();
builder.Services.AddHostedService<RetryWorker>();
builder.Services.AddHostedService<PoisonMessageWorker>();

var host = builder.Build();

await host.RunAsync();