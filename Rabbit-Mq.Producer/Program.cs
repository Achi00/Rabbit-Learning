using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Application.Infrastructure;
using RabbitMQ.Application.Interfaces.Messages;
using RabbitMQ.Application.Services.Messages;
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


var host = builder.Build();

await host.RunAsync();