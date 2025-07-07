namespace ShoppingCartMicroservice_Service.DTOs
{
    public class ShoppingCartItemDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal Price     { get; set; }  
        public decimal LineTotal => Price * Quantity;
    }
}
