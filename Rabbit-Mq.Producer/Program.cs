using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMq.Contracts;
using RabbitMQ.Application.Handlers.InventoryHandlers;
using RabbitMQ.Application.Handlers.PaymentHandlers;
using RabbitMQ.Application.Handlers.StockHandlers;
using RabbitMQ.Application.Infrastructure;
using RabbitMQ.Application.Interfaces.Messages;
using RabbitMQ.Application.Sagas;
using RabbitMQ.Application.Services.Interfaces.Messages;
using RabbitMQ.Application.Services.Messages;
using RabbitMQ.Application.Services.Messages.OrderHandlers;
using RabbitMQ.Application.Services.Messages.Orders;
using RabbitMQ.Application.Workers;
using RabbitMqDemo.Persistance.Context;

var builder = Host.CreateApplicationBuilder(args);

// dbcontext
builder.Services.AddDbContext<MessageDbContext>(options =>
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string not found");

    options.UseSqlServer(connectionString);
});

builder.Services.AddSingleton(new RabbitMqConnectionProvider("localhost"));

// worker
// writes to outbox
builder.Services.AddHostedService<OrderProducerWorker>();
// publishes from outbox
builder.Services.AddHostedService<OutboxRelayWorker>();

// outbox
builder.Services.AddScoped<IMessagePublisher, RabbitMqPublisher>();

builder.Services.AddScoped<OrderSagaCoordinator>();

var host = builder.Build();

await host.RunAsync();