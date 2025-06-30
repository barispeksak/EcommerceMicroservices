using ProductMicroservice.Service.DTOs;

namespace ProductMicroservice.Service.Interfaces;

/// <summary>
/// Ürünlere ait iş kurallarını dış katmanlara sunan sözleşme.
/// Controller, gRPC servisleri veya arka-plan job’ları yalnızca bu arayüzü görür.
/// </summary>
public interface IProductService
{
    /// <summary>Id ile tek ürünü döner.</summary>
    Task<ProductDto> GetAsync(int id);

    /// <summary>Tüm ürünleri listeler.</summary>
    Task<IEnumerable<ProductDto>> GetAllAsync();

    /// <summary>Yeni ürün ekler.</summary>
    Task<ProductDto> CreateAsync(CreateProductDto dto);

    /// <summary>Var olan ürünü günceller (null olmayan alanları uygular).</summary>
    Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto);

    /// <summary>Ürünü siler.</summary>
    Task DeleteAsync(int id);
}
