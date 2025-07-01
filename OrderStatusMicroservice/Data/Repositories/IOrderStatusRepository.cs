using OrderStatusMicroservice.Entities;

namespace OrderStatusMicroservice.Data.Repositories
{
    public interface IOrderStatusRepository
    {
        Task<IEnumerable<OrderStatus>> GetAllAsync();
        Task<OrderStatus?> GetByIdAsync(int id);
        Task<OrderStatus> AddAsync(OrderStatus entity);
        Task<bool> UpdateAsync(OrderStatus OrderStatus);
        Task<bool> DeleteAsync(int id);
    }
}
