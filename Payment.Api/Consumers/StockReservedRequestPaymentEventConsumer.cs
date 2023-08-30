using MassTransit;
using Shared.Events;
using Shared.Interfaces;

namespace Payment.Api.Consumers
{
    public class StockReservedRequestPaymentEventConsumer : IConsumer<IStockReservedRequestPaymentEvent>
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<StockReservedRequestPaymentEventConsumer> _logger;

        public StockReservedRequestPaymentEventConsumer(IPublishEndpoint publishEndpoint, ILogger<StockReservedRequestPaymentEventConsumer> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IStockReservedRequestPaymentEvent> context)
        {
            var balance = 3000m;

            if (balance > context.Message.Payment.TotalPrice)
            {
                _logger.LogInformation($"{context.Message.Payment.TotalPrice}₺ was withrawn from credit card for userId: {context.Message.BuyerId}");

                await _publishEndpoint.Publish(new PaymentCompletedEvent(context.Message.CorrelationId));
            }
            else
            {
                _logger.LogInformation($"{context.Message.Payment.TotalPrice}₺ was not withrawn from credit card for userId: {context.Message.BuyerId}");

                await _publishEndpoint.Publish(new PaymentFailedEvent(context.Message.CorrelationId)
                {
                    Reason = "Not enough balance",
                    OrderItems = context.Message.OrderItems,
                });
            }
        }
    }
}
