namespace Order.Api.Dtos
{
    public class OrderCreateDto
    {
        public OrderCreateDto()
        {
            OrderItems = new List<OrderItemDto>();
        }

        public int BuyerId { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
        public PaymentDto Payment { get; set; }
        public AddressDto Address { get; set; }
    }
}
