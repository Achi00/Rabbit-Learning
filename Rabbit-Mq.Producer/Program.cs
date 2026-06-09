using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Application.Infrastructure;
using RabbitMQ.Application.Workers;
using RabbitMQ.Client;
using RabbitMqDemo.Persistance.Context;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();

var builder = Host.CreateApplicationBuilder(args);

// dbcontext
builder.Services.AddDbContext<MessageDbContext>(options =>
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string not found");

    options.UseSqlServer(connectionString);
});

var connectionProvider = await RabbitMqConnectionProvider.CreateAsync("localhost");
builder.Services.AddSingleton(connectionProvider);
// worker
builder.Services.AddHostedService<OrderProducerWorker>();


var host = builder.Build();

await host.RunAsync();