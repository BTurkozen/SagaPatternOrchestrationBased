namespace Order.Api.Dtos
{
    public sealed class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
    }
}
