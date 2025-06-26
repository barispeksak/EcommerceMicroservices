namespace OrderSummaryMicroservice.DTOs
{
    public class OrderSummaryDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductItemId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}