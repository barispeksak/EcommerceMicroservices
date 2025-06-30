using System.ComponentModel.DataAnnotations;

namespace ProductConfigurationMicroservice_Data.Entities
{
    /// <summary>
    /// ProductItem × VariationOption eşleşmesini tutar.
    /// FK yok – diğer servislerle HTTP üzerinden doğrulanacak.
    /// </summary>
    public class ProductConfiguration
    {
        [Key]
        public int Id { get; set; }

        /// <summary>SKU’yu temsil eden ProductItem Id’si</summary>
        public int ProductItemId { get; set; }

        /// <summary>Seçilen varyasyon (renk, beden, vb.) Id’si</summary>
        public int VariationOptionId { get; set; }
    }
}
