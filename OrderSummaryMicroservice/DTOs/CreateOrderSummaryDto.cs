namespace OrderSummaryMicroservice.DTOs
{
    public class CreateOrderSummaryDto
    {
        public int OrderId { get; set; }
        public int ProductItemId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}