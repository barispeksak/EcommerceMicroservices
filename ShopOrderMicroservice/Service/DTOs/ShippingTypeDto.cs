namespace ShopOrderMicroservice.Data.Dtos
{
    public class ShippingTypeDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
