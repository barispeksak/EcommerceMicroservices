namespace ShoppingCartMicroservice_Service.DTOs
{
    public class CreateShoppingCartDto
    {
        public int CartId        { get; set; } 
        public int ProductItemId { get; set; }
        public int Qty { get; set; }
    }
}
