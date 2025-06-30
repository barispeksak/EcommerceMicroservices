// ProductConfigurationMicroservice_Service/Interfaces/IProductConfigurationService.cs
using ProductConfigurationMicroservice_Service.DTOs;

namespace ProductConfigurationMicroservice_Service.Interfaces;

/// <summary>
/// Yalın ihtiyaç: listele, tekil getir, sil.
/// SKU ve VariationOption adlarını doldurma sorumluluğu implementasyonda olacak.
/// </summary>
public interface IProductConfigurationService
{
    /// <param name="productItemId">
    /// İsteğe bağlı filtre – sadece bu SKU’ya ait konfigürasyonları getirir.
    /// null geçildiğinde tüm kayıtlar döner.
    /// </param>
    Task<IEnumerable<ProductConfigurationDto>> GetAllAsync(int? productItemId = null);

    /// <returns>Bulunmazsa null döner.</returns>
    Task<ProductConfigurationDto?> GetByIdAsync(int id);

    /// <summary>Kaydı siler; yoksa <c>KeyNotFoundException</c> fırlatır.</summary>
    Task DeleteAsync(int id);
}
