using OrderSummaryMicroservice.DTOs;

namespace OrderSummaryMicroservice.Services
{
    public interface IOrderSummaryService
    {
        Task<IEnumerable<OrderSummaryDto>> GetAllAsync();
        Task<OrderSummaryDto?> GetByIdAsync(int id);
        Task<OrderSummaryDto> CreateAsync(CreateOrderSummaryDto createOrderSummaryDto);
        Task<OrderSummaryDto?> UpdateAsync(int id, UpdateOrderSummaryDto updateOrderSummaryDto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}