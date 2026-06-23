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
    /*
    * used in case we need standalone consumer logic or when something needs to react
    * on same event independently, without saga,
    * those consumers replaced my custom handlers and bg workers
    */
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

        // not global configurations
        cfg.ReceiveEndpoint("fulfillment-queue", e => 
        {
            e.UseMessageRetry(r => r.Exponential(
                retryLimit: 3,
                minInterval: TimeSpan.FromSeconds(5),
                maxInterval: TimeSpan.FromSeconds(5),
                intervalDelta: TimeSpan.FromSeconds(30)
            ));

            e.ConfigureConsumer<OrderSubmittedConsumer>(ctx);
        });

        // global configuration, TEST LATER
        //cfg.UseMessageRetry(r => r.Exponential(3,
        //TimeSpan.FromSeconds(5),
        //TimeSpan.FromMinutes(5),
        //TimeSpan.FromSeconds(30)));

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
