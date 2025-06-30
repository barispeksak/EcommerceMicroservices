using ProductConfigurationMicroservice_Data.Entities;

namespace ProductConfigurationMicroservice_Data.Repositories;

/// <summary>
/// Sadece listeleme, tekil getirme ve silme işlemleri.
/// CRUD’un “C” ve “U” kısmı şimdilik yok – ihtiyacın olursa ekle.
/// </summary>
public interface IProductConfigurationRepository
{
    /// <summary>Filtreli veya tam liste döner.</summary>
    Task<IEnumerable<ProductConfiguration>> GetAllAsync(int? productItemId = null);

    /// <summary>Id’si verilen satırı getirir; yoksa null.</summary>
    Task<ProductConfiguration?> GetByIdAsync(int id);

    /// <summary>Satırı silmek için marklar (SaveAsync çağrısına kadar DB’ye işlenmez).</summary>
    Task RemoveAsync(ProductConfiguration entity);

    /// <summary>Değişiklikleri veritabanına yazar.</summary>
    Task SaveAsync();
}
