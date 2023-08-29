using MassTransit;
using Shared.Messages;

namespace Shared.Interfaces
{
    public interface IStockReservedRequestPaymentEvent : CorrelatedBy<Guid>
    {
        // Bu event'ı Payment dinleyecek.
        public PaymentMessage Payment { get; set; }

        // Payment işleminde bir sorun yaşanırsa İşlemleri geri alabilmek için burada OrderItem'ları tutuyoruz.
        public List<OrderItemMessage> OrderItem { get; set; }
    }
}
