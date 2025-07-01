using ProductConfigurationMicroservice_Service.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductConfigurationMicroservice_Service.Interfaces
{
    public interface IProductConfigurationService
    {
        /// <summary>Çoklu filtreyle konfigürasyon listesi.</summary>
        Task<IEnumerable<ProductConfigurationDto>> GetAllAsync(
            int[]? productItemIds,
            int[]? variationOptionIds);

        /// <summary>ID’ye göre tekil konfigürasyon.</summary>
        Task<ProductConfigurationDto?> GetByIdAsync(int id);

        /// <summary>Yeni konfigürasyon ekler.</summary>
        Task<ProductConfigurationDto> AddAsync(CreateProductConfigurationDto dto);

        /// <summary>Mevcut konfigürasyonu günceller.</summary>
        Task UpdateAsync(UpdateProductConfigurationDto dto);

        /// <summary>Konfigürasyonu siler.</summary>
        Task DeleteAsync(int id);
    }
}
