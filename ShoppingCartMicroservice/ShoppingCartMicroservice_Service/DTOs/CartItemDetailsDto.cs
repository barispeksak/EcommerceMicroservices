namespace ShoppingCartMicroservice_Service.DTOs
{
    public class CartItemDetailsDto
    {
        public int     Id       { get; set; }   // ProductItemId
        public string  Sku      { get; set; } = null!;
        public int     Quantity { get; set; }
        public decimal Price    { get; set; }
        public string  Currency { get; set; } = null!;
        public string? Name     { get; set; }   // nullable → fallback “Bilinmiyor”
        public string? Image    { get; set; }   // nullable → fallback boş string
    }
}
