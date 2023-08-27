using MassTransit;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SagaStateMachineWorkerService.Models
{
    public class OrderStateInstance : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public int BuyerId { get; set; }
        /// <summary>
        /// Odeme tamamlandıktan sonra order service'e bir event gönderilecek ve Status'unu completed olarak ayarlıyacak.
        /// </summary>
        public int OrderId { get; set; }
        public string CardName { get; set; }
        public string CardNumber { get; set; }
        public string Expiration { get; set; }
        public string CVV { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
        public DateTime CreatedOn { get; set; }
        public override string ToString()
        {
            var properties = GetType().GetProperties();

            var sb = new StringBuilder();

            properties.ToList().ForEach(p =>
            {
                var value = p.GetValue(this, null);

                sb.Append($"{p.Name}:{value}");
            });

            sb.Append("-----------------------");

            return sb.ToString();
        }
    }
}
