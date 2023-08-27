using Shared.Interfaces;
using Shared.Messages;

namespace Shared.Events
{
    public class OrderCreatedRequestEvent : IOrderCreatedRequestEvent
    {
        public OrderCreatedRequestEvent()
        {
            OrderItems = new List<OrderItemMessage>();
        }

        public int OrderId { get; set; }
        public string BuyerId { get; set; }
        public List<OrderItemMessage> OrderItems { get; set; }
        public PaymentMessage Payment { get; set; }
    }
}
