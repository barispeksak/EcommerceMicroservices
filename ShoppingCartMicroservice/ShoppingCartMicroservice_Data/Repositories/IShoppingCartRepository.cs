using ShoppingCartMicroservice_Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShoppingCartMicroservice_Service.Interfaces
{
    public interface IShoppingCartRepository
    {
        Task<ShoppingCart> AddAsync(ShoppingCart entity);
        Task UpdateAsync(ShoppingCart entity);
        Task DeleteAsync(int id);
        Task<ShoppingCart> GetByIdAsync(int id);
        Task<List<ShoppingCart>> GetByCartIdAsync(int cartId);
        Task<ShoppingCart> GetTotalRowByCartIdAsync(int cartId);
    }
}
