using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMq.Contracts;
using RabbitMQ.Application.Infrastructure;
using RabbitMQ.Application.Interfaces.Messages;
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

// not connected at construct time, only when it is first used
builder.Services.AddSingleton(new RabbitMqConnectionProvider("localhost"));

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