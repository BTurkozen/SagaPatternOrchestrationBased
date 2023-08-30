using MassTransit;
using Shared.Events;
using Shared.Interfaces;
using Shared.Messages;
using Shared.Settings;

namespace SagaStateMachineWorkerService.Models
{
    public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
    {
        public Event<IOrderCreatedRequestEvent> OrderCreatedRequestEvent { get; set; }
        public State OrderCreated { get; private set; }

        public Event<IStockReservedEvent> StockReservedEvent { get; set; }
        public State StockReserved { get; private set; }

        public Event<IPaymentCompletedEvent> PaymentCompletedEvent { get; set; }
        public State PaymentComplated { get; private set; }

        public Event<IStockNotReservedEvent> StockNotReservedEvent { get; set; }
        public State StockNotReserved { get; private set; }

        public Event<IPaymentFailedEvent> PaymentFailedEvent { get; set; }
        public State PaymentFailed { get; set; }

        public OrderStateMachine()
        {
            // ilk state'imiz initial olması gerekiyor.
            // Initial'ı currentstate içerisine ekliyoruz.
            InstanceState(i => i.CurrentState);

            // Aynı OrderId'ye ait başka bir satır varmı yokmu kontrol ediyoruz.
            // OrderStateInstance nesnesindeki orderId ile OrderCreatedRequestEvent'taki orderId karşılaştırılıyor.
            // Karşılaştırma sonucu var ise birşey yapma. yok ise yeni Guid Id ile oluştur.
            Event(() =>
                OrderCreatedRequestEvent, y => y.CorrelateBy<int>(c => c.OrderId, z => z.Message.OrderId).SelectId(context => Guid.NewGuid()));

            Event(() => StockReservedEvent, x => x.CorrelateById(c => c.Message.CorrelationId));

            Event(() => PaymentCompletedEvent, x => x.CorrelateById(c => c.Message.CorrelationId));

            Event(() => StockNotReservedEvent, x => x.CorrelateById(c => c.Message.CorrelationId));

            // Initial evresinden OrderCreated evresine geçilecek.
            // Bunu burada belirtmemiz gerekmektedir.
            // When => OrderCreatedRequestEvent'e geldiyse
            // Then => Business kodlarını çalıştıracağımız yer.
            // TransitionTo => Hangi evreye geçsin diyoruz. State Durumu
            Initially(When(OrderCreatedRequestEvent).Then(context =>
            {
                // Gelen Event'deki data ile dolduralacak.
                // Instance veri tabanına kayıt edilecek satırı temsil etmekte. 
                // Instance => Veritabanı temsil eder.
                // Data => Gelen Event'daki datayı temsil eder.
                context.Instance.BuyerId = context.Data.BuyerId;
                context.Instance.OrderId = context.Data.OrderId;
                context.Instance.CreatedOn = DateTime.Now;
                context.Instance.CardName = context.Data.Payment.CardName;
                context.Instance.CardNumber = context.Data.Payment.CardNumber;
                context.Instance.CVV = context.Data.Payment.CVV;
                context.Instance.Expiration = context.Data.Payment.Expiration;
                context.Instance.TotalPrice = context.Data.Payment.TotalPrice;
            }).Then(context =>
            {
                Console.WriteLine($"OrderCreatedRequestEvent before: {context.Instance}");
            })
            // OrderCreated Event'ı gönderme işlemi gerçekleştiriyoruz.
            // Event içerisini Instance'dan da Data'dan da doldurabiliriz. Biz burada Data ile doldurma işlemi gerçekleştirdirk.
            // Publish yapıldığı gibi bunu bir de dinleyen olması gerekmektedir.Bu da Stock Microservicedir.
            .Publish(context => new OrderCreatedEvent(context.Instance.CorrelationId)
            {
                OrderItems = context.Data.OrderItems
            })
            .TransitionTo(OrderCreated)
            .Then(context =>
            {
                Console.WriteLine($"OrderCreatedRequestEvent after: {context.Instance}");
            }));

            // StateMachine içerisinden gönderilen Eventların corelationId'si bulunmalıdır.
            // Hangi staır ile ilgili hangi Instance ile ilgili olduğunu tespit etmesi için gereklidir.
            // Bu işlem için Masstransit'de "CorrelatedBy<Guid>" adında bir sınıf bulunmakta. Event'a kalıtım yoluyla implement edersek bu özelliği kazandırmış oluruz.

            // OrderCreated State'indeyken,StockReservedEvent event'i gelirse Stock işlemi tamamlandı. Artık Odeme işlemini başlatabiliriz.
            During(OrderCreated,
                   When(StockReservedEvent)
                    .TransitionTo(StockReserved)
                    .Send(new Uri($"queue:{RabbitMqSettingsConst.StockReservedRequestPaymentQueueName}"), context => new StockReservedRequestPaymentEvent(context.Instance.CorrelationId)
                    {
                        OrderItems = context.Data.OrderItems,
                        Payment = new Shared.Messages.PaymentMessage
                        {
                            CardName = context.Instance.CardName,
                            CardNumber = context.Instance.CardNumber,
                            CVV = context.Instance.CVV,
                            Expiration = context.Instance.Expiration,
                            TotalPrice = context.Instance.TotalPrice,
                        },
                        BuyerId = context.Instance.BuyerId
                    }).Then(context =>
                    {
                        Console.WriteLine($"StockReservedQueueName after: {context.Instance}");
                    }), When(StockNotReservedEvent).TransitionTo(StockNotReserved).Publish(context => new OrderRequestFailedEvent
                    {
                        OrderId = context.Instance.OrderId,
                        Reason = context.Data.Reason
                    }).Then(context =>
                    {
                        Console.WriteLine($"StockNotReservedQueueName after: {context.Instance}");
                    }));

            During(StockReserved, When(PaymentCompletedEvent).TransitionTo(PaymentComplated).Publish(context => new OrderRequestCompletedEvent
            {
                OrderId = context.Instance.OrderId
            }).Then(context =>
            {
                Console.WriteLine($"PaymentCompletedEvent after: {context.Instance}");
            }).Finalize(), When(PaymentFailedEvent).Publish(context => new OrderRequestFailedEvent
            {
                OrderId = context.Instance.OrderId,
                Reason = context.Data.Reason
            }).Send(new Uri($"queue:{RabbitMqSettingsConst.StockRollbackMessageQueueName}"), context =>
                new StockRollbackMessage()
                {
                    OrderItems = context.Data.OrderItems,
                }).TransitionTo(PaymentFailed).Then(context =>
                {
                    Console.WriteLine($"StockRollbackMessage after: {context.Instance}");
                }));

            // Final evresine gelen dataları sil komutu
            SetCompletedWhenFinalized();
        }
    }
}
