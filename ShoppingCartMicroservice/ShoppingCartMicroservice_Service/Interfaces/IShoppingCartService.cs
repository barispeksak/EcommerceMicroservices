using ShoppingCartMicroservice_Service.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShoppingCartMicroservice_Service.Interfaces
{
    public interface IShoppingCartService
    {
        Task<ShoppingCartSummaryDto> GetAllItemsAsync(int cartId);               // Sepetin tüm ürünleri ve toplam fiyat
        Task<int> GetItemQuantityAsync(int productItemId, int cartId);           // Aynı üründen kaç tane var?
        Task<ShoppingCartDto> AddItemAsync(CreateShoppingCartDto dto, int cartId); // Ürün ekle
        Task UpdateCartAsync(UpdateShoppingCartDto dto, int cartId);               // Sepeti topluca güncelle
        Task DeleteItemAsync(int id);                                             // Ürün sil
    }
}
