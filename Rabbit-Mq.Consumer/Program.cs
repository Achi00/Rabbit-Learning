using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMq.Contracts;
using RabbitMQ.Application.Handlers.InventoryHandlers;
using RabbitMQ.Application.Handlers.PaymentHandlers;
using RabbitMQ.Application.Handlers.StockHandlers;
using RabbitMQ.Application.Infrastructure;
using RabbitMQ.Application.Sagas;
using RabbitMQ.Application.Services;
using RabbitMQ.Application.Services.Interfaces;
using RabbitMQ.Application.Services.Interfaces.Messages;
using RabbitMQ.Application.Services.Messages.Idempotency;
using RabbitMQ.Application.Services.Messages.Orders;
using RabbitMQ.Application.Workers;
using RabbitMqDemo.Persistance.Context;

var builder = Host.CreateApplicationBuilder(args);

// registed db context
builder.Services.AddDbContext<MessageDbContext>(options =>
{
    // using string for testing purpouse, in future will be use nameof and classes
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("Connection string not found");

    options.UseSqlServer(connectionString);
});

//var connectionProvider = await RabbitMqConnectionProvider.CreateAsync("localhost");
builder.Services.AddScoped<OrderSagaCoordinator>();
// not connected at construct time, only when it is first used
builder.Services.AddSingleton(new RabbitMqConnectionProvider("localhost"));

builder.Services.AddKeyedScoped<IMessageHandler, OrderCreatedHandler>(MessageTypes.OrderCreated);
builder.Services.AddKeyedScoped<IMessageHandler, OrderCancelledHandler>(MessageTypes.OrderCancelled);
builder.Services.AddKeyedScoped<IMessageHandler, InventoryHandler>(MessageTypes.ReserveStock);
builder.Services.AddKeyedScoped<IMessageHandler, PaymentHandler>(MessageTypes.ChargePayment);
builder.Services.AddKeyedScoped<IMessageHandler, ReleaseStockHandler>(MessageTypes.ReleaseStock);
// Workers
builder.Services.AddHostedService<TopologySetup>();
builder.Services.AddHostedService<OrderConsumerWorker>();
builder.Services.AddHostedService<RetryWorker>();
builder.Services.AddHostedService<PoisonMessageWorker>();
builder.Services.AddScoped<IOrderCreateProcessor, FakeOrderProcessor>();


// Services
//builder.Services.AddScoped<IOrderCreateProcessor, FakeOrderProcessor>();
//builder.Services.AddScoped<IOrderCancelProcessor, FakeOrderCancelProcessor>();

// Idempotancy service
builder.Services.AddScoped<DbIdempotencyService>();

var host = builder.Build();

await host.RunAsync();