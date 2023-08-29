using Shared.Interfaces;
using Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events
{
    public class StockReservedRequestPaymentEvent : IStockReservedRequestPaymentEvent
    {
        public StockReservedRequestPaymentEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        public PaymentMessage Payment { get; set; }
        public List<OrderItemMessage> OrderItem { get; set; }

        public Guid CorrelationId { get; }
    }
}
