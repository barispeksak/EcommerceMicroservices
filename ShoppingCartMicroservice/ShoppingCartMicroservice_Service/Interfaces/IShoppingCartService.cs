using ShoppingCartMicroservice_Service.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShoppingCartMicroservice_Service.Interfaces
{
    public interface IShoppingCartService
    {
        Task<List<CartItemDetailsDto>> GetCartDetailsForUser(string userId);
        Task AddOrUpdateItemAsync(string userId, CreateShoppingCartDto dto);
        Task RemoveItemAsync(string userId, int productItemId);
        Task ClearAsync(string userId);
        
    }
}
