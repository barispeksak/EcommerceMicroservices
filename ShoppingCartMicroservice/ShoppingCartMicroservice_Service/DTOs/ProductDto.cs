namespace ShoppingCartMicroservice_Service.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Brand { get; set; } = null!;
        public string Image { get; set; } = null!;   // JSON'da field "image"
    }
}