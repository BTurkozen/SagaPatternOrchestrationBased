using MassTransit;
using Order.Api.Models;
using Shared.Interfaces;

namespace Order.Api.Consumers
{
    public class OrderRequestCompletedEventConsumer : IConsumer<IOrderRequestCompletedEvent>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<OrderRequestCompletedEventConsumer> _logger;

        public OrderRequestCompletedEventConsumer(DataContext dataContext, ILogger<OrderRequestCompletedEventConsumer> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IOrderRequestCompletedEvent> context)
        {
            var order = await _dataContext.Orders.FindAsync(context.Message.OrderId);

            if (order is not null)
            {
                order.Status = Status.Complete;
                await _dataContext.SaveChangesAsync();

                _logger.LogInformation($"OrderId: {context.Message.OrderId} status changed: {order.Status}");
            }
            else
            {
                _logger.LogError($"OrderId: {context.Message.OrderId} not found");
            }
        }
    }
}
