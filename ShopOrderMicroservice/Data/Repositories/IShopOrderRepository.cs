using ShopOrderMicroservice.Models;

namespace ShopOrderMicroservice.Repositories
{
    public interface IShopOrderRepository
    {
        Task<IEnumerable<ShopOrder>> GetAllAsync();
        Task<ShopOrder?> GetByIdAsync(int id);
        Task AddAsync(ShopOrder order);
        Task<bool> UpdateAsync(ShopOrder order);
        Task<bool> DeleteAsync(int id);
    }
}
