using ShopOrderMicroservice.DTOs;

namespace ShopOrderMicroservice.Services
{
    public interface IShopOrderService
    {
        Task<IEnumerable<ShopOrderDto>> GetShopOrders();
        Task<ShopOrderDto> GetShopOrder(int id);
        Task<ShopOrderDto> CreateShopOrder(ShopOrderDto shopOrderDto);
        Task<ShopOrderDto> UpdateShopOrder(ShopOrderDto shopOrderDto);
        Task<bool> DeleteShopOrder(int id);
    }
}
