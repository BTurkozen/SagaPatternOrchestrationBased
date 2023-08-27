using System.Net;

namespace Order.Api.Models
{
    public class Order
    {
        public Order()
        {
            OrderItems = new List<OrderItem>();
        }

        public int Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public int BuyerId { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        public Status Status { get; set; }
        public string Fail { get; set; }
        public Address Address { get; set; }
    }

    public enum Status
    {
        Suspend,
        Complete,
        Fail
    }
}