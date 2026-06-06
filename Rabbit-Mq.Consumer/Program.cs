using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Application.Infrastructure;
using RabbitMQ.Application.Services;
using RabbitMQ.Application.Services.Interfaces;
using RabbitMQ.Application.Services.Interfaces.Messages;
using RabbitMQ.Application.Services.Messages;
using RabbitMQ.Application.Workers;

var builder = Host.CreateApplicationBuilder(args);

var connectionProvider = await RabbitMqConnectionProvider.CreateAsync("localhost");
builder.Services.AddSingleton(connectionProvider);

// Workers
builder.Services.AddHostedService<TopologySetup>();
builder.Services.AddHostedService<OrderConsumerWorker>();
builder.Services.AddHostedService<RetryWorker>();
builder.Services.AddHostedService<PoisonMessageWorker>();
builder.Services.AddScoped<IOrderProcessor, FakeOrderProcessor>();
// type handlers
builder.Services.AddKeyedScoped<IMessageHandler, OrderCreatedHandler>("OrderCreated");
//builder.Services.AddKeyedScoped<IMessageHandler, OrderCancelledHandler>("OrderCancelled");

var host = builder.Build();

await host.RunAsync();