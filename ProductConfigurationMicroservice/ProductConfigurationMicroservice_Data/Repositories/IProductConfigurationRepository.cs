using ProductConfigurationMicroservice_Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductConfigurationMicroservice_Data.Repositories
{
    /// <summary>
    /// CRUD + çoklu filtre desteği.
    /// </summary>
    public interface IProductConfigurationRepository
    {
        /// <summary>
        /// Eski tekli filtre hâlâ geçerli (geri-dönüşüm).
        /// </summary>
        Task<IEnumerable<ProductConfiguration>> GetAllAsync(int? productItemId = null);

        /// <summary>
        /// Çoklu ProductItem & VariationOption filtreleri (AND).
        /// Parametreler null veya boş gelirse o kriter uygulanmaz.
        /// </summary>
        Task<IEnumerable<ProductConfiguration>> GetAllAsync(
            int[]? productItemIds,
            int[]? variationOptionIds);

        Task<ProductConfiguration?> GetByIdAsync(int id);

        Task AddAsync(ProductConfiguration entity);
        Task UpdateAsync(ProductConfiguration entity);

        Task RemoveAsync(ProductConfiguration entity);
        Task SaveAsync();
    }
}
