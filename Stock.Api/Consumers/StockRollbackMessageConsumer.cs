using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Messages;
using Stock.Api.Models;

namespace Stock.Api.Consumers
{
    public class StockRollbackMessageConsumer : IConsumer<IStockRollbackMessage>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<StockRollbackMessageConsumer> _logger;

        public StockRollbackMessageConsumer(DataContext dataContext, ILogger<StockRollbackMessageConsumer> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IStockRollbackMessage> context)
        {

            foreach (var orderItem in context.Message.OrderItems)
            {
                var stock = await _dataContext.Stocks.FirstOrDefaultAsync(s => s.ProductId == orderItem.ProductId);

                if (stock is not null)
                {
                    stock.Count += orderItem.Count;

                    await _dataContext.SaveChangesAsync();
                }
            }

            _logger.LogInformation($"Stock was released");
        }
    }
}
