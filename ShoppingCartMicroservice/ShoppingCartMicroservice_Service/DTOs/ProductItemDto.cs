namespace ShoppingCartMicroservice_Service.DTOs
{
    public class ProductItemDto
    {
        public int Id { get; set; }      // ProductItem.Id  (sepette tutulacak)
        public int ProductId { get; set; }      // İlişkili Product
        public decimal Price { get; set; }
        public string  Sku             { get; set; } = null!;

        public int QuantityInStock { get; set; } 
        public string Currency { get; set; } = null!;

        public ProductDto Product { get; set; } = null!;   // Nested product detayları
    }
}