using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrdersAPI.Consumer;
using OrdersAPI.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OrdersAPIContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrdersAPIContext")
        ?? throw new InvalidOperationException("Connection string 'OrdersAPIContext' not found.")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---- MassTransit + RabbitMQ (récepteur) ----
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProductCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(builder.Configuration["RabbitMq:Host"]!), h =>
        {
            h.Username(builder.Configuration["RabbitMq:Username"]!);
            h.Password(builder.Configuration["RabbitMq:Password"]!);
        });

        cfg.ReceiveEndpoint("event-listener", e =>
        {
            e.ConfigureConsumer<ProductCreatedConsumer>(context);
        });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
