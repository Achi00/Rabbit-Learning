using MassTransit;
using Microsoft.EntityFrameworkCore;
using RabbitMq.Infrastructure.Messaging.Consumers;
using RabbitMq.Infrastructure.Messaging.Saga;
using RabbitMQ.Application.Sagas;
using RabbitMqDemo.Persistance.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(x =>
{
    // register consumers
    // should implament IConsumer<T>
    x.AddConsumer<OrderSubmittedConsumer>();
    //x.AddConsumer<ChargePaymentConsumer>();
    //x.AddConsumer<ReleaseStockConsumer>();

    // register saga
    x.AddSagaStateMachine<OrderStateMachine, OrderSagaState>()
        .EntityFrameworkRepository(r =>
        {
            r.UseSqlServer();
            r.ExistingDbContext<MessageDbContext>();
        });

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("localhost");
        cfg.ConfigureEndpoints(ctx);
    });
});

// add db context
builder.Services.AddDbContext<MessageDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
