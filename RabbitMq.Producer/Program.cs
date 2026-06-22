using MassTransit;
using RabbitMqDemo.Persistance.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// masstransit
builder.Services.AddMassTransit(x => 
{
    x.AddEntityFrameworkOutbox<MessageDbContext>(c =>
    {
        c.UseSqlServer();
        c.UseBusOutbox();
    });

    x.UsingRabbitMq((ctx, cfg) => 
    {
        cfg.Host("localhost");
        // replaces my prev TopologySetup class
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
