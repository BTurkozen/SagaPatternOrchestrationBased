using MassTransit;
using Shared.Interfaces;

namespace Stock.Api.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<IOrderCreatedEvent>
    {
        public Task Consume(ConsumeContext<IOrderCreatedEvent> context)
        {
            throw new NotImplementedException();
        }
    }
}
