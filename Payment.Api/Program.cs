using MassTransit;
using Payment.Api.Consumers;
using Shared.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(options =>
{
    options.AddConsumer<StockReservedRequestPaymentEventConsumer>();

    options.UsingRabbitMq((context, config) =>
    {
        config.Host(builder.Configuration.GetConnectionString("RabbitMq"));

        config.ReceiveEndpoint(RabbitMqSettingsConst.StockReservedRequestPaymentQueueName, e =>
        {
            e.ConfigureConsumer<StockReservedRequestPaymentEventConsumer>(context);
        });
    });
});

var logger = builder.Services.BuildServiceProvider().GetService<ILogger<StockReservedRequestPaymentEventConsumer>>();

builder.Services.AddSingleton(typeof(ILogger), logger);

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
