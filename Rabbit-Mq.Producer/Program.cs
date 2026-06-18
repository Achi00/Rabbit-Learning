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
using RabbitMQ.Application.Workers;
using RabbitMQ.Client;
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

// worker handlers
// passes command and tells what needs to happend, publishes result event
builder.Services.AddKeyedScoped<IMessageHandler, InventoryHandler>(MessageTypes.ReserveStock);
builder.Services.AddKeyedScoped<IMessageHandler, PaymentHandler>(MessageTypes.ChargePayment);
builder.Services.AddKeyedScoped<IMessageHandler, ReleaseStockHandler>(MessageTypes.ReleaseStock);

// relay handlers
// passes event and tells what already happend, forwards to coordinator
// naming in past tense: Reserved, Failed, Released...
builder.Services.AddKeyedScoped<IMessageHandler, StockReservedHandler>(MessageTypes.StockReserved);
builder.Services.AddKeyedScoped<IMessageHandler, StockReservationFailedHandler>(MessageTypes.StockReservationFailed);
builder.Services.AddKeyedScoped<IMessageHandler, PaymentChargedHandler>(MessageTypes.PaymentCharged);
builder.Services.AddKeyedScoped<IMessageHandler, PaymentFailedHandler>(MessageTypes.PaymentFailed);
builder.Services.AddKeyedScoped<IMessageHandler, StockReleasedHandler>(MessageTypes.StockReleased);


var host = builder.Build();

await host.RunAsync();