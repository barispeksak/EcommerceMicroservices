using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShopOrderMicroservice.Data.Dtos;

namespace ShopOrderMicroservice.Services.Interfaces
{
    public interface IShopOrderService
    {
        Task<IEnumerable<ShopOrderDto>> GetAllAsync();
        Task<ShopOrderDto?> GetByIdAsync(int id);
        Task<ShopOrderDto?> CreateAsync(CreateShopOrderDto dto);
        Task<bool> UpdateAsync(int id, UpdateShopOrderDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<ShopOrderDto>> GetByUserIdAsync(int userId);
        Task<IEnumerable<ShopOrderDto>> GetByDateRangeAsync(DateTime start, DateTime end);
    }
}