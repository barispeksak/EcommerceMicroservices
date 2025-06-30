namespace ProductItemMicroservice_Service.DTOs
{
    /// <summary>
    /// Ürün stok detay DTO'su. Tüm alanlar readonly.
    /// </summary>
    public class ProductItemDto
    {
        /// <example>12345-KIRMIZI-M</example>
        public string Sku { get; set; }

        /// <example>10</example>
        public int QuantityInStock { get; set; }

        /// <example>499.99</example>
        public decimal Price { get; set; }

        /// <example>TRY</example>
        public string Currency { get; set; }

        /// <example>123</example>
        public int ProductId { get; set; }

        /// <example>1</example>
        public int Id { get; set; }
    }
}
