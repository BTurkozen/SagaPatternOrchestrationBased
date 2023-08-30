using MassTransit;
using Order.Api.Models;
using Shared.Interfaces;

namespace Order.Api.Consumers
{
    public class OrderRequestFailedEventConsumer : IConsumer<IOrderRequestFailedEvent>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<OrderRequestFailedEventConsumer> _logger;

        public OrderRequestFailedEventConsumer(DataContext dataContext, ILogger<OrderRequestFailedEventConsumer> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IOrderRequestFailedEvent> context)
        {
            var order = await _dataContext.Orders.FindAsync(context.Message.OrderId);

            if (order is not null)
            {
                order.Status = Status.Fail;
                order.Fail = context.Message.Reason;
                await _dataContext.SaveChangesAsync();

                _logger.LogInformation($"OrderId: {context.Message.OrderId} status changed: {order.Status}");
            }
            else
            {
                _logger.LogInformation($"OrderId: {context.Message.OrderId} not found");
            }
        }
    }
}
