using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductItemMicroservice_Data.Entities
{
    public class ProductItem
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// SKU (Stok Kodu). Büyük harf, rakam ve tire içermelidir. Maksimum 30 karakter.
        /// Örnek: "12345-KIRMIZI-M" veya "9876-SIYAH-L"
        /// Format: [PRODUCT_ID]-[RENK]-[BEDEN]
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string Sku { get; set; }

        /// <summary>
        /// Stoktaki ürün adedi. Sıfırdan küçük olamaz.
        /// </summary>
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stok adedi negatif olamaz.")]
        public int QuantityInStock { get; set; }

        /// <summary>
        /// Ürün fiyatı. Sıfırdan büyük olmalı.
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat sıfırdan büyük olmalı.")]
        public decimal Price { get; set; }

        /// <summary>
        /// Para birimi. Sadece "TRY", "USD" veya "EUR" olabilir.
        /// </summary>
        [Required]
        [MaxLength(3)]
        public string Currency { get; set; }

        /// <summary>
        /// İlgili ürünün kimliği. Pozitif olmalı.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "ProductId pozitif olmalı.")]
        public int ProductId { get; set; }
    }
}
