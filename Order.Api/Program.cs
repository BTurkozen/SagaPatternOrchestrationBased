using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.Api.Consumers;
using Order.Api.Models;
using Shared.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionStr"));
});

builder.Services.AddMassTransit(options =>
{
    options.AddConsumer<OrderRequestCompletedEventConsumer>();
    options.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMq"));

        cfg.ReceiveEndpoint(RabbitMqSettingsConst.OrderRequestCompletedQueueName, e =>
        {
            e.ConfigureConsumer<OrderRequestCompletedEventConsumer>(ctx);
        });
    });
});

var logger = builder.Services.BuildServiceProvider().GetService<ILogger<OrderRequestCompletedEventConsumer>>();

builder.Services.AddSingleton(typeof(ILogger), logger);
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
