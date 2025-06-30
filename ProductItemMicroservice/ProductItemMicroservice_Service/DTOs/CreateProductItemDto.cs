namespace ProductItemMicroservice_Service.DTOs
{
    /// <summary>
    /// Yeni ürün stoğu ekleme/güncelleme için DTO.
    /// </summary>
    public class CreateProductItemDto
    {
        /// <summary>
        /// SKU (Stok Kodu). Sadece büyük harf, rakam ve tire içermelidir. Max 30 karakter.
        /// Örnek: 12345-KIRMIZI-M
        /// </summary>
        public string Sku { get; set; }

        /// <summary>
        /// Stok adedi. Sıfırdan küçük olamaz. Örnek: 10
        /// </summary>
        public int QuantityInStock { get; set; }

        /// <summary>
        /// Fiyat. Sıfırdan büyük olmalı. Örnek: 499.99
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Para birimi. Sadece TRY, USD, EUR girilebilir. Örnek: TRY
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Ürünün ID'si. Pozitif olmalı. Örnek: 123
        /// </summary>
        public int ProductId { get; set; }
    }
}
