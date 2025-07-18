using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EntityShopOrder = ShopOrderMicroservice.Data.Entities.ShopOrder;

namespace ShopOrderMicroservice.Data.Repositories
{
    public interface IShopOrderRepository
    {
        Task<IEnumerable<EntityShopOrder>> GetAllAsync();
        Task<EntityShopOrder?> GetByIdAsync(int id);
        Task AddAsync(EntityShopOrder order);
        Task<bool> UpdateAsync(EntityShopOrder updatedOrder);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<EntityShopOrder>> GetByUserIdAsync(int userId);
        Task<IEnumerable<EntityShopOrder>> GetByDateRangeAsync(DateTime start, DateTime end);
    }
}