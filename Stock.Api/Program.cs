using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Settings;
using Stock.Api.Consumers;
using Stock.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseInMemoryDatabase("SagaChoreagraphyStockDb");
});

builder.Services.AddMassTransit(options =>
{
    options.AddConsumer<OrderCreatedEventConsumer>();

    options.AddConsumer<StockRollbackMessageConsumer>();

    options.UsingRabbitMq((context, config) =>
    {
        config.Host(builder.Configuration.GetConnectionString("RabbitMq"));

        config.ReceiveEndpoint(RabbitMqSettingsConst.StockOrderCreatedEventQueueName, e =>
        {
            e.ConfigureConsumer<OrderCreatedEventConsumer>(context);
        });

        config.ReceiveEndpoint(RabbitMqSettingsConst.StockRollbackMessageQueueName, e =>
        {
            e.ConfigureConsumer<StockRollbackMessageConsumer>(context);
        });
    });
});

var logger = builder.Services.BuildServiceProvider().GetService<ILogger<StockRollbackMessageConsumer>>();

builder.Services.AddSingleton(typeof(ILogger), logger);

var app = builder.Build();


using var scope = app.Services.CreateScope();

var serviceProvider = scope.ServiceProvider;

var dataContext = serviceProvider.GetRequiredService<DataContext>();

if (dataContext.Stocks.Any() is false)
{
    dataContext.Stocks.Add(new Stock.Api.Models.Stock() { Id = 1, ProductId = 1, Count = 100 });

    dataContext.Stocks.Add(new Stock.Api.Models.Stock() { Id = 2, ProductId = 2, Count = 200 });

    dataContext.SaveChanges();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
