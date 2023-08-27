using MassTransit;
using Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagaStateMachineWorkerService.Models
{
    public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
    {
        public Event<IOrderCreatedRequestEvent> OrderCreatedRequestEvent { get; set; }
        public State OrderCreated { get; private set; }


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
            }).TransitionTo(OrderCreated).Then(context =>
            {
                Console.WriteLine($"OrderCreatedRequestEvent after: {context.Instance}");
            }));
        }
    }
}
