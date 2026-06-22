using MassTransit;
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
            r.AddDbContext<MessageDbContext>();
        });

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("localhost");
        cfg.ConfigureEndpoints(ctx);
    });
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
