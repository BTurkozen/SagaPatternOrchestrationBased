using Shared.Interfaces;
using Shared.Messages;

namespace Shared.Events
{
    public class OrderCreatedEvent : IOrderCreatedEvent
    {
        public List<OrderItemMessage> OrderItems { get; set; }
    }
}
