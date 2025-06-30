using ProductItemMicroservice_Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductItemMicroservice_Data.Repositories
{
    public interface IProductItemRepository
    {
        Task<ProductItem> GetByIdAsync(int id);
        Task<List<ProductItem>> GetAllAsync();
        Task<ProductItem> GetBySkuAsync(string sku);
        Task AddAsync(ProductItem entity);
        Task UpdateAsync(ProductItem entity);
        Task DeleteAsync(ProductItem entity);
        Task<bool> SkuExistsAsync(string sku);
    }
}
