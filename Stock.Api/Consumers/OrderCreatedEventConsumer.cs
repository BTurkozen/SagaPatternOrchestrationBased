using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using Shared.Interfaces;
using Shared.Settings;
using Stock.Api.Models;

namespace Stock.Api.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<IOrderCreatedEvent>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger _logger;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderCreatedEventConsumer(DataContext dataContext, ILogger<OrderCreatedEventConsumer> logger, ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint)
        {
            _dataContext = dataContext;
            _logger = logger;
            _sendEndpointProvider = sendEndpointProvider;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<IOrderCreatedEvent> context)
        {
            var stockResult = new List<bool>();

            foreach (var orderItem in context.Message.OrderItems)
            {
                stockResult.Add(await _dataContext.Stocks.AnyAsync(s => s.ProductId == orderItem.ProductId && s.Count > orderItem.Count));
            }

            if (stockResult.All(sr => sr.Equals(true)))
            {
                foreach (var orderItem in context.Message.OrderItems)
                {
                    var stock = await _dataContext.Stocks.FirstOrDefaultAsync(oi => oi.ProductId == orderItem.ProductId);

                    if (stock is not null)
                    {
                        stock.Count -= orderItem.Count;

                    }
                    await _dataContext.SaveChangesAsync();

                }
                _logger.LogInformation($"Stock was reserved for Correlation Id : {context.Message.CorrelationId}");
                // Burada gönderilen event'ları gene statemachine dinleyecek.
                var stockReservedEvent = new StockReservedEvent(context.Message.CorrelationId)
                {
                    OrderItems = context.Message.OrderItems
                };

                await _publishEndpoint.Publish(stockReservedEvent);
            }
            else
            {
                await _publishEndpoint.Publish(new StockNotReservedEvent(context.Message.CorrelationId)
                {
                    Reason = "Not enough stock"
                });

                _logger.LogInformation($"Not enough stock for Correlation Id : {context.Message.CorrelationId}");
            }
        }
    }
}