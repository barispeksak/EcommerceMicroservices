// Services/Interfaces/IOrderStatusService.cs
using OrderStatusMicroservice.Data.Dtos;

namespace OrderStatusMicroservice.Services.Interfaces
{
    public interface IOrderStatusService
    {
        Task<IEnumerable<OrderStatusDto>> GetAllAsync();
        Task<OrderStatusDto?> GetByIdAsync(int id);
        Task<OrderStatusDto> CreateAsync(CreateOrderStatusDto dto);
        Task<bool> UpdateAsync(UpdateOrderStatusDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
